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

using System;

namespace WordsLive.Documents
{
	class PdfPresentationBridge
	{
		public event Action CallbackLoaded;

		private string document;
		private DocumentPageScale pageScale;
		private int transitionDuration;

		public PdfPresentationBridge(string document, DocumentPageScale pageScale, int transitionDuration)
		{
			this.document = document;
			this.pageScale = pageScale;
			this.transitionDuration = transitionDuration;
		}

		public string GetDocument()
		{
			return document;
		}

		public bool GetRenderWholePage()
		{
			return pageScale == DocumentPageScale.WholePage;
		}

		public int GetTransitionDuration()
		{
			return transitionDuration;
		}

		public void OnCallbackLoaded()
		{
			if (CallbackLoaded != null)
				CallbackLoaded();
		}
	}
}
