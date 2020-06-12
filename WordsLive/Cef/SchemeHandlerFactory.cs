/*
 * WordsLive - worship projection software
 * Copyright (c) 2020 Patrick Reisert
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

using CefSharp;
using System;
using System.IO;
using System.Reflection;
using WordsLive.Core;

namespace WordsLive.Cef
{
	public class SchemeHandlerFactory : ISchemeHandlerFactory
	{
		public const string SchemeName = "asset";

		public IResourceHandler Create(IBrowser browser, IFrame frame, string schemeName, IRequest request)
		{
			var uri = new Uri(request.Url);
			var fileName = uri.AbsolutePath.Substring(1);
			var fileExtension = Path.GetExtension(fileName);
			string mimeType = CefSharp.Cef.GetMimeType(Path.GetExtension(fileName));

			if (uri.Host == "backgrounds")
			{
				var bg = DataManager.Backgrounds.GetFile(Uri.UnescapeDataString(uri.AbsolutePath));
				if (bg.Uri.IsFile)
				{
					try
					{
						return ResourceHandler.FromFilePath(bg.Uri.LocalPath, mimeType);
					}
					catch
					{
						return null;
					}
				}
				else
				{
					// TODO: use WebClient to download the file?
					throw new NotImplementedException();
				}
			}

			if (uri.Host == "urimap")
			{
				return UriMapDataSource.Instance.CreateHandler(uri);
			}

			Assembly ass;
			switch (uri.Host.ToLower())
			{
				case "wordslive":
					ass = Assembly.GetExecutingAssembly();
					break;
				case "wordslive.core":
					ass = Assembly.GetAssembly(typeof(Core.Media));
					break;
				default:
					return null;
			}

			var stream = ass.GetManifestResourceStream(ass.GetName().Name + ".Resources." + fileName.Replace('/', '.'));
			return ResourceHandler.FromStream(stream, mimeType);
		}
	}
}