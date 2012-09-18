using System.Windows.Media;
using WordsLive.Core;

namespace WordsLive.MediaOrderList
{
	[TargetMedia(typeof(FileNotFoundMedia))]
	class FileNotFoundIconProvider : IconProvider
	{
		public FileNotFoundIconProvider(FileNotFoundMedia data) : base(data)
		{ }

		protected override ImageSource CreateIcon()
		{
			// TODO (Words): create an image (or XAML-resource?)
			return null;
		}
	}
}
