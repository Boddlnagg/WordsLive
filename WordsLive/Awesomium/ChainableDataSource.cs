/*
 * WordsLive - worship projection software
 * Copyright (c) 2013 Patrick Reisert
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

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
