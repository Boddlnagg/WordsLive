/*
 * WordsLive - worship projection software
 * Copyright (c) 2015 Patrick Reisert
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
using System.IO;
using System.Linq;
using CefSharp;
using Newtonsoft.Json;
using WordsLive.Cef;
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

		private IWebBrowser control;
		private bool loaded = false;
		private bool showChords = true;
		private FeatureLevel features;
		private SongDisplayBridge bridge;

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
					control.GetMainFrame().ExecuteJavaScriptAsync("presentation.setShowChords(" + JsonConvert.SerializeObject(showChords) + ")");
			}
		}

		/// <summary>
		/// Initializes a new instance of the SongDisplayController class.
		/// </summary>
		/// <param name="control">The web view that is controlled by this instance. Its IsProcessCreated property must be true.</param>
		/// <param name="features">The desired feature level.</param>
		public SongDisplayController(IWebBrowser control, FeatureLevel features = FeatureLevel.None)
		{
			this.control = control;
			this.features = features;
			this.control.ConsoleMessage += (obj, target) =>
			{
				System.Windows.MessageBox.Show("SongDisplayController encountered JS error in " + target.Source + " (line " +  target.Line + "): " + target.Message);
			};
			this.bridge = new SongDisplayBridge(features);

			control.JavascriptObjectRepository.UnRegisterAll();
			control.JavascriptObjectRepository.Register("bridge", bridge, true);
			bridge.CallbackLoaded += OnSongLoaded;
		}

		public void Load(Song song)
		{
			if (song == null)
				throw new ArgumentNullException("song");

			if (features != FeatureLevel.None)
			{
				var backgrounds = new List<string>(song.Backgrounds.Count);

				foreach (var bg in song.Backgrounds.Where(bg => bg.Type == SongBackgroundType.Image))
				{
					try
					{
						backgrounds.Add(bg.FilePath.Replace('\\', '/'));
					}
					catch (FileNotFoundException)
					{
						// ignore -> just show black background
					}
				}
				bridge.PreloadImages = backgrounds;
			}
			bridge.Song = song;
			bridge.ShowChords = ShowChords;
			
			this.control.Load("asset://WordsLive/song.html");
		}

		public void UpdateFormatting(SongFormatting formatting, bool hasTranslation, bool hasChords)
		{
			this.control.GetMainFrame().ExecuteJavaScriptAsync("presentation.updateFormatting(" + JsonConvert.SerializeObject(formatting) + ", " + JsonConvert.SerializeObject(hasTranslation) + ", " + JsonConvert.SerializeObject(hasChords) + ")");
		}

		public event EventHandler SongLoaded;

		protected void OnSongLoaded()
		{
			loaded = true;
			control.GetMainFrame().ExecuteJavaScriptAsync("presentation.setShowChords(" + JsonConvert.SerializeObject(showChords) + ")");

			if (SongLoaded != null)
				System.Windows.Application.Current.Dispatcher.Invoke(() => SongLoaded(this, EventArgs.Empty));
		}

		public void SetSource(SongSource source)
		{
			control.GetMainFrame().ExecuteJavaScriptAsync("presentation.setSource(" + JsonConvert.SerializeObject(source.ToString()) + ")");
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
				control.GetMainFrame().ExecuteJavaScriptAsync("presentation.showSource(" + JsonConvert.SerializeObject(showSource) + ")");
			}
		}

		public void SetCopyright(string copyright)
		{
			control.GetMainFrame().ExecuteJavaScriptAsync("presentation.setCopyright(" + JsonConvert.SerializeObject(copyright) + ")");
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
				control.GetMainFrame().ExecuteJavaScriptAsync("presentation.showCopyright(" + JsonConvert.SerializeObject(showCopyright) + ")");
			}
		}

		public void ShowSlide(SongSlide slide)
		{
			if (slide == null) return; // slide might be null if the song has no parts

			var s = new
			{
				Text = slide.Text,
				Translation = slide.TranslationWithoutChords, // TODO: does the display controller need to know about chords in the translation?
				Size = slide.Size,
				Background = slide.Background,
				Source = showSource,
				Copyright = showCopyright
			};

			control.GetMainFrame().ExecuteJavaScriptAsync("presentation.showSlide(" + JsonConvert.SerializeObject(s) + ")");
		}

		public void GotoSlide(SongPartReference part, int slide)
		{
			control.GetMainFrame().ExecuteJavaScriptAsync("presentation.gotoSlide("+part.OrderIndex+", "+slide+")");
		}

		public void GotoBlankSlide(SongBackground background)
		{
			control.GetMainFrame().ExecuteJavaScriptAsync("presentation.gotoBlankSlide("+JsonConvert.SerializeObject(background)+")");
		}
	}
}
