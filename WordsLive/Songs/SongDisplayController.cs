using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
			this.control.SetObjectProperty("bridge", "featureLevel", new JSValue(JsonConvert.SerializeObject(features)));
		}

		public void Load(Song song)
		{
			if (song == null)
				throw new ArgumentNullException("song");

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

			this.control.SetObjectProperty("bridge", "preloadImages", new JSValue(backgrounds.ToArray()));
			this.control.SetObjectProperty("bridge", "songString", new JSValue(JsonConvert.SerializeObject(song)));
			this.control.SetObjectProperty("bridge", "showChords", new JSValue(ShowChords));
			
			this.control.LoadFile("song.html");
		}

		public void UpdateFormatting(SongFormatting formatting, bool hasTranslation, bool hasChords)
		{
			this.control.ExecuteJavascript("presentation.updateFormatting(" + JsonConvert.SerializeObject(formatting) + ", " + JsonConvert.SerializeObject(hasTranslation) + ", " + JsonConvert.SerializeObject(hasChords) + ")");
		}

		public event EventHandler SongLoaded;

		protected void OnSongLoaded()
		{
			loaded = true;
			if (!ShowChords) // defaults to true
			{
				control.ExecuteJavascript("presentation.setShowChords(false)");
			}

			if (SongLoaded != null)
				SongLoaded(this, EventArgs.Empty);
		}

		public void SetSource(SongSource source)
		{
			control.ExecuteJavascript("presentation.setSource(" + JsonConvert.SerializeObject(source.ToString()) + ")");
		}

		private bool showSource;
		private bool showCopyright;

		public bool ShowSource
		{
			get
			{
				return showSource;
			}
			set
			{
				showSource = value;
				control.ExecuteJavascript("presentation.showSource(" + JsonConvert.SerializeObject(showSource) + ")");
			}
		}

		public void SetCopyright(string copyright)
		{
			control.ExecuteJavascript("presentation.setCopyright(" + JsonConvert.SerializeObject(copyright) + ")");
		}

		public bool ShowCopyright
		{
			get
			{
				return showCopyright;
			}
			set
			{
				showCopyright = value;
				control.ExecuteJavascript("presentation.showCopyright(" + JsonConvert.SerializeObject(showCopyright) + ")");
			}
		}

		public void ShowSlide(SongSlide slide)
		{
			var s = new
			{
				Text = slide.Text,
				Translation = slide.Translation,
				Size = slide.Size,
				Background = slide.Background,
				Source = showSource,
				Copyright = showCopyright
			};

			control.ExecuteJavascript("presentation.showSlide(" + JsonConvert.SerializeObject(s) + ")");
		}

		public void GotoSlide(SongPartReference part, int slide)
		{
			control.ExecuteJavascript("presentation.gotoSlide("+part.OrderIndex+", "+slide+")");
		}

		public void GotoBlankSlide(SongBackground background)
		{
			control.ExecuteJavascript("presentation.gotoBlankSlide("+JsonConvert.SerializeObject(background)+")");
		}
	}
}
