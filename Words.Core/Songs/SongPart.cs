using System;
using System.Collections.Generic;
using System.Linq;

namespace Words.Core.Songs
{
	public class SongPart
	{
		public string Name { get; set; }
		
		public IList<SongSlide> Slides { get; set; }

		public SongPart()
		{
			this.Name = string.Empty;
			this.Slides = new List<SongSlide>();
		}
		
		public string Text
		{
			get
			{
				return string.Join("\n", Slides.Select(slide => slide.Text).ToArray());
			}
		}

		public string TextWithoutChords
		{
			get
			{
				return string.Join("\n", Slides.Select(slide => slide.TextWithoutChords).ToArray());
			}
		}

		public bool HasTranslation
		{
			get
			{
				foreach (SongSlide slide in this.Slides)
				{
					if (!String.IsNullOrEmpty(slide.Translation))
						return true;
				}
				return false;
			}
		}
	}
}
