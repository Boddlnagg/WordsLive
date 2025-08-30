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
using System.Collections.Generic;

namespace WordsLive.Documents
{
	public class PdfDocument : DocumentMedia
	{
		public PdfDocument(Uri uri, Dictionary<string, string> options) : base(uri, options) { }

		public override IDocumentPresentation CreatePresentation()
		{
			var pres = Controller.PresentationManager.CreatePresentation<PdfPresentation>();
			pres.Document = this;
			return pres;
		}
	}
}
