namespace WordsLive.Server.Utils.WebSockets
{
	public enum WebSocketCloseStatus
	{
		Empty = 0,
		NormalClosure = 1000,
		EndpointUnavailable	= 1001,
		ProtocolError = 1002,
		InvalidMessageType = 1003,
		MessageTooBig = 1004,
		InvalidPayloadData = 1007,
		PolicyViolation = 1008,
		MandatoryExtension = 1010,
		InternalServerError = 1011		
	}
}
