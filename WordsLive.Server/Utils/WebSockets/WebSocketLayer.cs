using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WordsLive.Server.Utils.WebSockets
{
	using System.Text;
	using Owin.Types;
	using WordsLive.Core;
	using WebSocketCloseAsync =
					Func
					<
						int /* closeStatus */,
						string /* closeDescription */,
						CancellationToken /* cancel */,
						Task
					>;
	using WebSocketReceiveAsync =
				Func
				<
					ArraySegment<byte> /* data */,
					CancellationToken /* cancel */,
					Task
					<
						Tuple
						<
							int /* messageType */,
							bool /* endOfMessage */,
							int /* count */
						>
					>
				>;
	using WebSocketReceiveTuple =
				Tuple
				<
					int /* messageType */,
					bool /* endOfMessage */,
					int /* count */
				>;
	using WebSocketSendAsync =
						Func
						<
							ArraySegment<byte> /* data */,
							int /* messageType */,
							bool /* endOfMessage */,
							CancellationToken /* cancel */,
							Task
						>;

	// This class implements the WebSocket layer on top of an opaque stream.
	// WebSocket Extension v0.4 is currently implemented.
	public class WebSocketLayer
	{
		private Stream stream;

		private IDictionary<string, object> environment;

		class WebSocketHeader
		{
			public bool Fin = false;
			public byte[] MaskingKey = {0, 0, 0, 0};
			public ulong PayloadLength = 0;
			public bool Mask = false;
			public MessageType OpCode;
			public ulong PayloadReceived = 0;

			public bool IsControlFrame
			{
				get
				{
					return ((int)OpCode & 0x8) == 0x8;
				}
			}

			public void UnmaskData(ArraySegment<byte> data)
			{
				for (var index = 0; index < data.Count; index++)
				{
					data.Array[data.Offset + index] = (byte)(data.Array[data.Offset + index] ^ this.MaskingKey[index & 0x03]);
				}
			}
		}

		enum MessageType : byte
		{
			Continuation = 0x0,
			Text = 0x1,
			Binary = 0x2,
			Close = 0x8,
			Ping = 0x9,
			Pong = 0xA
		}

		public WebSocketLayer(IDictionary<string, object> opaqueEnv)
		{
			this.environment = opaqueEnv;
			this.environment[OwinConstants.WebSocket.SendAsync] = new WebSocketSendAsync(SendAsync);
			this.environment[OwinConstants.WebSocket.ReceiveAsync] = new WebSocketReceiveAsync(ReceiveAsync);
			this.environment[OwinConstants.WebSocket.CloseAsync] = new WebSocketCloseAsync(CloseAsync);
			this.environment[OwinConstants.WebSocket.CallCancelled] = this.environment[OwinConstants./*Opaque.*/CallCancelled];
			this.environment[OwinConstants.WebSocket.Version] = "1.0";

			this.stream = (Stream)this.environment[OwinConstants.Opaque.Stream];
		}

		public IDictionary<string, object> Environment
		{
			get { return environment; }
		}

		// Add framing and send the data.  One frame per call to Send.
		public async Task SendAsync(ArraySegment<byte> buffer, int messageType, bool endOfMessage, CancellationToken cancel)
		{
			int headerLength = 0;
			byte len = 0;

			if (buffer.Count <= 125)
			{
				headerLength = 2;
				len = (byte)buffer.Count;
			}
			else if (buffer.Count <= ushort.MaxValue)
			{
				headerLength = 4;
				len = 126;
			}
			else
			{
				headerLength = 10;
				len = 127;
			}

			byte[] frame = new byte[headerLength + buffer.Count];
			frame[0] = (byte)((endOfMessage ? 0x80 : 0x00) | (byte)messageType);
			frame[1] = len;

			if (buffer.Count > 125)
			{
				if (buffer.Count <= ushort.MaxValue)
				{
					var data = BitConverter.GetBytes(Convert.ToUInt16(buffer.Count));
					frame[2] = data[1];
					frame[3] = data[0];
				}
				else
				{
					var data = BitConverter.GetBytes(Convert.ToUInt64(buffer.Count));
					frame[2] = data[7];
					frame[3] = data[6];
					frame[4] = data[5];
					frame[5] = data[4];
					frame[6] = data[3];
					frame[7] = data[2];
					frame[8] = data[1];
					frame[9] = data[0];
				}
			}

			Array.Copy(buffer.Array, buffer.Offset, frame, headerLength, buffer.Count);

			await stream.WriteAsync(frame, 0, headerLength + buffer.Count, cancel);
		}

		private WebSocketHeader currentHeader = null;

		// Receive frames, unmask them.
		// Should handle pings/pongs internally.
		// Should parse out Close frames.
		public async Task<WebSocketReceiveTuple> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancel)
		{
			if (currentHeader == null)
			{
				var header = new WebSocketHeader();
				byte headerReadState = 1;// skip initialization

				byte[] headerBuffer = new byte[14];
				int headerBytesRead = 0; 
				int headerLength = 2;

				while (true)
				{
					if (headerReadState == 0) // re-initialize
					{
						// create buffer big enough to hold the complete header
						headerBuffer = new byte[14];
						headerBytesRead = 0;
						headerLength = 2;
						headerReadState = 1;
					}

					var count = await stream.ReadAsync(headerBuffer, headerBytesRead, headerLength - headerBytesRead, cancel);
					headerBytesRead += count;
					if (headerBytesRead < headerLength)
					{
						continue;
					}

					if (headerReadState == 1) // read first 2 bytes
					{
						header.Fin = ((headerBuffer[0] >> 7) & 0x01) == 1;
						header.OpCode = (MessageType)((headerBuffer[0] >> 0) & 0x0f);
						header.Mask = ((headerBuffer[1] >> 7) & 0x01) == 1;
						headerReadState = 2;
					}

					if (headerReadState == 2) // read length
					{
						ulong len = (ulong)(headerBuffer[1] >> 0) & 0x7f;

						if (header.PayloadLength == 126)
						{
							headerLength = 4;
							if (headerBytesRead < headerLength)
							{
								continue;
							}
							len = (ulong)(headerBuffer[2] * 0x100) + headerBuffer[3];
						}
						else if (len == 127)
						{
							headerLength = 10;
							if (headerBytesRead < headerLength)
							{
								continue;
							}
							len = (ulong)(headerBuffer[6] * 0x1000000) +
									(ulong)(headerBuffer[7] * 0x10000) +
										(ulong)(headerBuffer[8] * 0x100) + (ulong)headerBuffer[9];
						}

						header.PayloadLength = len;
						headerReadState = 3; // length received
					}

					if (header.Mask && headerReadState >= 3) // read masking-key
					{
						if (headerReadState == 3)
						{
							headerLength += 4;
							headerReadState = 4;
						}
					
						if (headerBytesRead < headerLength)
						{
							continue;
						}
						header.MaskingKey[0] = headerBuffer[headerLength - 4];
						header.MaskingKey[1] = headerBuffer[headerLength - 3];
						header.MaskingKey[2] = headerBuffer[headerLength - 2];
						header.MaskingKey[3] = headerBuffer[headerLength - 1];
					}

					// received header
					Console.WriteLine("fin:{0} opcode:{1} mask:{2} len:{3} controlframe:{4}", header.Fin, header.OpCode, header.Mask, header.PayloadLength, header.IsControlFrame);

					if (header.IsControlFrame)
					{
						// See RFC 6455 section 5.5
						// can safely cast to int because control frames have max payload length of 125
						int controlFrameLength = (int)header.PayloadLength;
						var controlFrameBuffer = new byte[controlFrameLength];
						var controlFrameBytesRead = 0;

						while (controlFrameBytesRead < controlFrameLength)
						{
							controlFrameBytesRead += await stream.ReadAsync(controlFrameBuffer, controlFrameBytesRead, controlFrameLength - controlFrameBytesRead, cancel);
						}

						header.UnmaskData(new ArraySegment<byte>(controlFrameBuffer, 0, controlFrameLength));

						// TODO: read data (ping/pong-content)

						switch (header.OpCode)
						{
							case MessageType.Close:
								if (controlFrameLength >= 2)
								{
									this.Environment[OwinConstants.WebSocket.ClientCloseStatus] = (controlFrameBuffer[0] * 0x100) + controlFrameBuffer[1];
									if (controlFrameLength > 2)
									{
										this.Environment[OwinConstants.WebSocket.ClientCloseDescription] = Encoding.UTF8.GetString(controlFrameBuffer, 2, controlFrameLength - 2);
									}
								}
								return new WebSocketReceiveTuple((int)MessageType.Close, true, 0);
							case MessageType.Ping:
							case MessageType.Pong:
								throw new NotImplementedException(); // TODO
						}

						headerReadState = 0; // receive next header
					}
					else
					{
						currentHeader = header;
						break;
					}
				}
			}

			// now receive payload
			int maxBytes;
			if (currentHeader.PayloadLength > int.MaxValue)
				maxBytes = int.MaxValue;
			else
				maxBytes = (int)currentHeader.PayloadLength;

			if (buffer.Count < maxBytes)
				maxBytes = buffer.Count;

			int bytes = await stream.ReadAsync(buffer.Array, buffer.Offset, maxBytes, cancel);

			// unmask it
			if (currentHeader.Mask)
			{
				// TODO: handle unaligned offset?
				currentHeader.UnmaskData(buffer);
			}

			currentHeader.PayloadReceived += (ulong)bytes;

			bool endOfFrame = currentHeader.PayloadReceived >= currentHeader.PayloadLength;
			bool endOfMessage = endOfFrame && currentHeader.Fin;

			int messageType = (int)currentHeader.OpCode;

			if (endOfFrame)
				currentHeader = null; // forget last header

			return new WebSocketReceiveTuple(messageType, endOfMessage, bytes);
		}

		// Send a close frame. The WebSocket is not actually considered closed until a close frame has been both sent and received.
		public async Task CloseAsync(int status, string description, CancellationToken cancel)
		{
			var d1 = BitConverter.GetBytes(Convert.ToUInt16(status));
			byte[] d2 = null;
			Array.Reverse(d1);
			int len = 2;

			if (!string.IsNullOrEmpty(description))
			{
				d2 = Encoding.UTF8.GetBytes(description);
				len += d2.Length;
			}
			var data = new ArraySegment<byte>(new byte[len]);
			Array.Copy(d1, data.Array, 2);
			if (d2 != null)
			{
				Array.Copy(d2, 0, data.Array, 2, data.Array.Length);
			}
			await SendAsync(data, (int)MessageType.Close, true, cancel);
		}

		// Shutting down.  Send a close frame if one has been received but not set. Otherwise abort (fail the Task).
		public /*async*/ Task CleanupAsync()
		{
			return TaskHelpers.FromError(new NotImplementedException());
		}
	}
}
