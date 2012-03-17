using Words.Core;

#if FALSE
namespace Words.Slideshow.Impress
{
	[FileExtension(".odp")]
	public class ImpressMedia : Media
	{
		protected override bool LoadFromMetadata()
		{
			return true; // TODO (Slideshow.Impress): check if impress is available
		}
	}
}
#endif
