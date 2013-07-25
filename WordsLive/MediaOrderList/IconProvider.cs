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

using System.Windows.Media;

using WordsLive.Core;

namespace WordsLive.MediaOrderList
{
	public class IconProvider
	{
		private Media data;
		private ImageSource cached;

		public IconProvider(Media data)
		{
			this.data = data;
		}

		public ImageSource Icon
		{
			get
			{
				if (cached == null)
					cached = CreateIcon();
				return cached;
			}
		}

		public void Invalidate()
		{
			cached = null;
		}

		protected virtual ImageSource CreateIcon()
		{
			ImageSource icon;

			if (this.Data.Uri != null && this.Data.Uri.IsFile)
			{
				try
				{
					using (System.Drawing.Icon sysicon = System.Drawing.Icon.ExtractAssociatedIcon(this.Data.Uri.LocalPath))
					{
						icon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
									sysicon.Handle,
									System.Windows.Int32Rect.Empty,
									System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
					}
				}
				catch
				{
					icon = null;
				}
			}
			else
			{
				icon = null;
			}

			return icon;
		}

		protected Media Data
		{
			get
			{
				return data;
			}
		}
	}
}
