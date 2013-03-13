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

		public void Update()
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
