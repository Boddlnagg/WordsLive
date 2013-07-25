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
using System.IO;
using Xps = System.Windows.Xps.Packaging;

namespace WordsLive.Documents
{
	public class XpsDocument : DocumentMedia
	{
		public Xps.XpsDocument Document { get; private set; }

		public XpsDocument(Uri uri) : base(uri) { }

		public override void Load()
		{
			if (!this.Uri.IsFile)
				throw new NotImplementedException("Loading XPS document from remote URI not implemented yet.");

			Document = new Xps.XpsDocument(this.Uri.LocalPath, FileAccess.Read);
		}

		public override IDocumentPresentation CreatePresentation()
		{
			var pres = Controller.PresentationManager.CreatePresentation<XpsPresentation>();
			pres.SetSourceDocument(this);
			return pres;
		}
	}
}
