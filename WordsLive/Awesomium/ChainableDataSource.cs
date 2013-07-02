using System;
using Awesomium.Core.Data;

namespace WordsLive.Awesomium
{
	public class ChainableDataSource : DataSource, IDataSource
	{
		private IDataSource first;
		private IDataSource next;

		public ChainableDataSource(IDataSource first, IDataSource next)
		{
			this.first = first;
			this.next = next;
		}

		public ChainableDataSource(IDataSource single)
		{
			this.first = single;
			this.next = null;
		}

		protected override void OnRequest(DataSourceRequest request)
		{
			try
			{
				var handled = HandleRequest(request, (response) => SendResponse(request, response));
				if (!handled)
				{
					SendRequestFailed(request);
				}
			}
			catch
			{
				SendRequestFailed(request);
			}
		}

		public bool HandleRequest(DataSourceRequest request, Action<DataSourceResponse> respond)
		{
			var handled = first.HandleRequest(request, respond);
			if (handled || next == null)
			{
				return handled;
			}
			else
			{
				return next.HandleRequest(request, respond);
			}
		}
	}
}
