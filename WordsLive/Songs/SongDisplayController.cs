using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Awesomium.Core;
using Newtonsoft.Json;
using WordsLive.Core;
using WordsLive.Core.Data;
using WordsLive.Core.Songs;

namespace WordsLive.Songs
{
	/// <summary>
	/// Class to interact with the song.html page and the javascript there (and in the referenced SongPresentation.js)
	/// </summary>
	public class SongDisplayController
	{
		public enum FeatureLevel
		{
			None,
			Backgrounds,
			Transitions
		}

		private IWebViewJavaScript control;
		private bool loaded = false;
		private bool showChords = true;

		public bool ShowChords
		{
			get
			{
				return showChords;
			}
			set
			{
				showChords = value;
				if (loaded)
					control.ExecuteJavascript("presentation.setShowChords("+JsonConvert.SerializeObject(showChords)+")");
			}
		}

		public SongDisplayController(IWebViewJavaScript control, FeatureLevel features = FeatureLevel.None)
		{
			this.control = control;

			// TODO: unpack resources only once at startup
			var asm = Assembly.GetAssembly(typeof(Media)); // WordsLive.Core.dll
			var names = asm.GetManifestResourceNames();

			using (var stream = asm.GetManifestResourceStream("WordsLive.Core.Resources.jquery.js"))
			{
				using (var writer = new FileStream(Path.Combine("Data", "jquery.js"), FileMode.Create))
				{
					stream.CopyTo(writer);
				}
			}

			using (var stream = asm.GetManifestResourceStream("WordsLive.Core.Resources.SongPresentation.js"))
			{
				using (var writer = new FileStream(Path.Combine("Data", "SongPresentation.js"), FileMode.Create))
				{
					stream.CopyTo(writer);
				}
			}

			this.control.CreateObject("bridge");
			this.control.SetObjectCallback("bridge", "callbackLoaded", (sender, args) => OnSongLoaded());

			if (features == FeatureLevel.Backgrounds || features == FeatureLevel.Transitions)
			{
				this.control.SetObjectProperty("bridge", "enableBackgrounds", new JSValue(true));

				if (features == FeatureLevel.Transitions)
				{
					this.control.SetObjectProperty("bridge", "enableTransitions", new JSValue(true));
				}
			}
		}

		public void Load(Song song)
		{
			var backgrounds = new List<JSValue>(song.Backgrounds.Count);

			foreach (var bg in song.Backgrounds.Where(bg => bg.Type == SongBackgroundType.Image))
			{
				try
				{
					backgrounds.Add(new JSValue(DataManager.Backgrounds.GetFile(bg).Uri.AbsoluteUri));
				}
				catch (FileNotFoundException)
				{
					// ignore -> just show black background
				}
			}

			var songData = new {
				HasChords = song.HasChords,
				HasTranslation = song.HasTranslation,
				Formatting = song.Formatting
			};

			this.control.SetObjectProperty("bridge", "preloadImages", new JSValue(backgrounds.ToArray()));
			this.control.SetObjectProperty("bridge", "songString", new JSValue(JsonConvert.SerializeObject(songData)));
			this.control.SetObjectProperty("bridge", "showChords", new JSValue(ShowChords));
			
			this.control.LoadFile("song.html");
		}

		public void UpdateCss(Song song, int width)
		{
			// TODO: remove?
		}

		public event EventHandler SongLoaded;

		protected void OnSongLoaded()
		{
			loaded = true;

			if (SongLoaded != null)
				SongLoaded(this, EventArgs.Empty);
		}

		public void SetSource(SongSource source)
		{
			control.ExecuteJavascript("presentation.setSource(" + JsonConvert.SerializeObject(source.ToString()) + ")");
		}

		public void ShowSource(bool show)
		{
			// TODO
		}

		public void SetCopyright(string copyright)
		{
			control.ExecuteJavascript("presentation.setCopyright(" + JsonConvert.SerializeObject(copyright) + ")");
		}

		public void ShowCopyright(bool show)
		{
			// TODO
		}

		private class Slide
		{
			public string Text { get; set; }
			public string Translation { get; set; }
			public int Size { get; set; }
			public object Background { get; set; }
			public bool Copyright { get; set; }
			public bool Source { get; set; }
		}

		public void UpdateSlide(Song song, SongSlide slide, bool updateBackground = true)
		{
			// TODO
		}

		public void UpdateSlide(Song song, SongSlide slide, bool showSource, bool showCopyright)
		{
			//using(StreamWriter writer = new StreamWriter("output.html"))
			//    writer.WriteLine(HtmlToString(GenerateSlideHtml(song, slide)));

			Slide s;
			
			if (slide != null)
			{
				s = new Slide
				{
					Text = slide.Text,
					Translation = slide.Translation,
					Size = slide.Size,
					Background = slide.Background,
					Source = showSource,
					Copyright = showCopyright
				};
			}
			else
			{
				s = new Slide
				{
					Text = "",
					Translation = "",
					Size = song.Formatting.MainText.Size,
					Background = song.Backgrounds[song.FirstSlide != null ? song.FirstSlide.BackgroundIndex : 0],
					Source = false,
					Copyright = false
				};
			}

			control.ExecuteJavascript("presentation.showSlide(" + JsonConvert.SerializeObject(s) + ")");
		}
	}
}
