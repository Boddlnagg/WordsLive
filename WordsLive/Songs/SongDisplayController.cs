/*
 * WordsLive - worship projection software
 * Copyright (c) 2014 Patrick Reisert
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
using Awesomium.Core;
using Newtonsoft.Json;
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

		private IWebView control;
		private bool loaded = false;
		private bool showChords = true;
		private JSObject bridge;
		private FeatureLevel features;

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
					control.ExecuteJavascript("presentation.setShowChords(" + JsonConvert.SerializeObject(showChords) + ")");
			}
		}

		/// <summary>
		/// Initializes a new instance of the SongDisplayController class.
		/// </summary>
		/// <param name="control">The web view that is controlled by this instance. Its IsProcessCreated property must be true.</param>
		/// <param name="features">The desired feature level.</param>
		public SongDisplayController(IWebView control, FeatureLevel features = FeatureLevel.None)
		{
			this.control = control;
			this.features = features;
			this.control.ConsoleMessage += (obj, target) =>
			{
				System.Windows.MessageBox.Show("SongDisplayController encountered JS error in " + target.Source + " (line " +  target.LineNumber + "): " + target.Message);
			};
			// FIXME: sometimes throws exception saying that "bridge" object already exists
			bridge = this.control.CreateGlobalJavascriptObject("bridge");
			bridge.Bind("callbackLoaded", false, (sender, args) => OnSongLoaded());
			bridge["featureLevel"] = new JSValue(JsonConvert.SerializeObject(features));
		}

		public void Load(Song song)
		{
			if (song == null)
				throw new ArgumentNullException("song");

			if (features != FeatureLevel.None)
			{
				var backgrounds = new List<JSValue>(song.Backgrounds.Count);

				foreach (var bg in song.Backgrounds.Where(bg => bg.Type == SongBackgroundType.Image))
				{
					try
					{
						backgrounds.Add(new JSValue(bg.FilePath.Replace('\\', '/')));
					}
					catch (FileNotFoundException)
					{
						// ignore -> just show black background
					}
				}

				bridge["preloadImages"] = new JSValue(backgrounds.ToArray());
				bridge["backgroundPrefix"] = new JSValue("asset://backgrounds/");
			}
			bridge["songString"] = new JSValue(JsonConvert.SerializeObject(song));
			bridge["showChords"] = new JSValue(ShowChords);
			
			this.control.Source = new Uri("asset://WordsLive/song.html");
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
