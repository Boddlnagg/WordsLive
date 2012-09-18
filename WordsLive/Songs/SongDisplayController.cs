using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using Words.Core.Songs;
using System.Globalization;
using Words.Core;
using Awesomium.Core;

namespace Words.Songs
{
	public class SongDisplayController
	{
		private IWebViewJavaScript control;
		private SongBackground lastBackground;

		private readonly string clsBack = "back";
		private readonly string clsFront = "front";
		private readonly string clsMain = "main";
		private readonly string clsTrans = "trans";
		private readonly string clsInner = "inner";
		private readonly string clsCopyright = "copyright";
		private readonly string clsSource = "source";

		private double factor;
		const double fontFactor = 1.3;
		const double mgFactor = 1.3;
		const double lhFactor = 1.28;

		public SongDisplayController(IWebViewJavaScript control)
		{
			this.control = control;

			this.control.CreateObject("callback");
			this.control.SetObjectCallback("callback", "imagesLoaded", (sender, args) => OnImagesLoaded());
		}

		public bool ShowChords { get; set; }

		public void UpdateCss(Song song, int width)
		{
			control.ExecuteJavascript("updateCss(" + PrepareJavascriptString(GenerateCss(song, width)) + ")");
		}

		public event EventHandler ImagesLoaded;

		protected void OnImagesLoaded()
		{
			if (ImagesLoaded != null)
				ImagesLoaded(this, EventArgs.Empty);
		}

		public void PreloadImages(IEnumerable<string> paths)
		{
			if (paths == null || paths.Count() < 1)
			{
				OnImagesLoaded();
				return;
			}

			var arg = from path in paths select PrepareJavascriptString("file://" + path).Replace('\\', '/');
			control.ExecuteJavascript("preloadImages([" + string.Join(",", arg) + "])");
		}

		public void SetSource(SongSource source)
		{
			control.ExecuteJavascript("setSource(" + PrepareJavascriptString(HtmlToString(GenerateSourceHtml(source))) + ")");
		}

		public void ShowSource(bool show)
		{ 
			control.ExecuteJavascript("showSource("+show.ToString().ToLower()+")");
		}

		public void SetCopyright(string copyright)
		{
			control.ExecuteJavascript("setCopyright(" + PrepareJavascriptString(HtmlToString(GenerateCopyrightHtml(copyright))) + ")");
		}

		public void ShowCopyright(bool show)
		{
			control.ExecuteJavascript("showCopyright(" + show.ToString().ToLower() + ")");
		}

		public void ChangeBackground(SongBackground bg, int fadeTime)
		{
			if (bg != lastBackground)
			{
				lastBackground = bg;

				string bgString = "";

				if (bg == null)
				{
					bgString = "background-color: transparent;";
				}
				if (bg.IsImage)
				{
					bgString = "background-color: black; background-image: url('file://" + (Path.Combine(MediaManager.BackgroundsDirectory, bg.ImagePath)).Replace('\\', '/') + "');";
					bgString += "background-repeat: no-repeat; background-size: 100%";
				}
				else
				{
					bgString = "background-color: " + MakeCssColor(bg.Color) + ";";
				}

				control.ExecuteJavascript("changeBackground(" + PrepareJavascriptString(bgString) + ", "+ fadeTime +")");
			}
		}

		public void UpdateSlide(Song song, SongSlide slide, bool updateBackground = true)
		{
			control.ExecuteJavascript("updateSlide(" + PrepareJavascriptString(HtmlToString(GenerateSlideHtml(song, slide))) + ")");

			if (updateBackground)
			{
				SongBackground bg;

				if (slide != null)
					bg = song.Backgrounds[slide.BackgroundIndex];
				else
					bg = song.Backgrounds[song.FirstSlide != null ? song.FirstSlide.BackgroundIndex : 0];

				ChangeBackground(bg, 0);
			}
		}

		private static string HtmlToString(IEnumerable<XElement> elements)
		{
			StringBuilder sb = new StringBuilder();
			foreach (XElement e in elements)
			{
				sb.Append(e.ToString());
			}
			return sb.ToString();
		}

		private IEnumerable<XElement> GenerateSourceHtml(SongSource source)
		{
			XElement sourceInner = new XElement("div", new XAttribute("class", "source"), source.ToString().Replace(" ", " "));

			yield return new XElement("div", new XAttribute("class", clsBack), sourceInner);
			yield return new XElement("div", new XAttribute("class", clsFront), sourceInner);
		}

		private IEnumerable<XElement> GenerateCopyrightHtml(string copyright)
		{
			var elements = from line in copyright.Split('\n') select new XElement("span", line.Replace(" ", " "));
			XElement copyrightInner = new XElement("div", new XAttribute("class", clsCopyright), elements.Count() > 0 ? (object)elements : String.Empty);
				
			yield return new XElement("div", new XAttribute("class", clsBack), copyrightInner);
			yield return new XElement("div", new XAttribute("class", clsFront), copyrightInner);
		}

		private string GetLineHeight(Song song)
		{
			double value;

			value = song.Formatting.MainText.Size;

			if (song.HasChords && ShowChords)
				value *= 1.5;

			if (song.HasTranslation)
				value += song.Formatting.TranslationLineSpacing;
			else
				value += song.Formatting.TextLineSpacing;

			return "height: " + MakeCssValue(value * lhFactor * factor, "px") + ";";
		}

		private IEnumerable<XElement> GenerateSlideHtml(Song song, SongSlide slide)
		{
			XElement inner = null;

			if (slide != null)
			{
				string fsText = "font-size: " + MakeCssValue(slide.Size * fontFactor * factor, "pt") + ";";

				inner = new XElement("div", new XAttribute("class", clsInner), new XAttribute("style", fsText));
				bool translation = !String.IsNullOrEmpty(slide.Translation);

				if (translation)
				{
					var lines = slide.Text.Split('\n');
					var transLines = slide.Translation.Split('\n');
					int lineCount = lines.Length >= transLines.Length ? lines.Length : transLines.Length;

					for (int i = 0; i < lineCount; i++)
					{
						string textLine;
						string transLine;

						if (i < lines.Length)
							textLine = lines[i];
						else
							textLine = String.Empty;

						if (i < transLines.Length)
							transLine = transLines[i];
						else
							transLine = String.Empty;

						inner.Add(new XElement("span", ParseLine(textLine, true)));
						inner.Add(new XElement("span", new XAttribute("class", clsTrans), ParseLine(transLine, false)));
					}
				}
				else
				{
					if (slide.Text != null)
					{
						foreach (string line in slide.Text.Split('\n'))
						{
							inner.Add(new XElement("span", ParseLine(line, true)));
						}
					}
				}
			}

			if (song.Formatting.IsOutlineEnabled || song.Formatting.IsShadowEnabled)
			{
				yield return new XElement("div", new XAttribute("class", clsBack),
					new XElement("div", new XAttribute("class", clsMain), (object)inner ?? String.Empty));
			}
			yield return new XElement("div", new XAttribute("class", clsFront),
				new XElement("div", new XAttribute("class", clsMain), (object)inner ?? String.Empty));
		}

		private IEnumerable<object> ParseLine(string line, bool chords)
		{
			string rest;

			if (String.IsNullOrEmpty(line))
				rest = String.Empty;
			else
				rest = "\uFEFF" + line.Replace(" ", " "); // not sure if we need the replace, but the \uFEFF (zero-width space)
														  // makes sure that the lines starts correcty
			
			List<object> elements = new List<object>();

			if (chords)
			{
				int i;

				while ((i = rest.IndexOf('[')) != -1)
				{
					string before = rest.Substring(0, i);
					int end = rest.IndexOf(']', i);
					if (end < 0)
						break;

					int next = rest.IndexOf('[', i + 1);
					if (next >= 0 && next < end)
					{
						elements.Add(before + "[");
						rest = rest.Substring(i + 1);
						continue;
					}

					string chord = rest.Substring(i + 1, end - (i + 1));

					rest = rest.Substring(end + 1);
					elements.Add(before);

					if (ShowChords)
					{
						// abusing the <b> tag for chords for brevity
						// we need two nested tags, the outer one with position:relative,
						// the inner one with position:absolute (see css below)
						elements.Add(new XElement("b", new XElement("b", chord)));
					}
				}
				
			}

			elements.Add(rest);
			return elements;
		}

		private static string PrepareJavascriptString(string value)
		{
			return "'" + value.Replace("'", "\\'").Replace('\r', ' ').Replace('\n', ' ') + "'";
		}

		private static string MakeCssColor(System.Drawing.Color color)
		{
			return "rgba(" + color.R + ", " + color.G + ", " + color.B + ", " + (double)color.A / 255 + ")";
		}

		private static string MakeCssValue(double val, string unit)
		{
			return val.ToString(CultureInfo.InvariantCulture) + unit;
		}

		private string GenerateCss(Song song, int width)
		{
			// TODO (WordsLive - song formatting): use Powerpraise's song settings for stroke and shadow size?
			SongFormatting formatting = song.Formatting;

			if (song.HasTranslation && formatting.TranslationPosition == TranslationPosition.Block)
				throw new NotImplementedException("Translation block positioning is not yet supported"); // TODO (WordsLive - song formatting): support translation block positioning

			factor = (double)width / 1024;
			double strokeFactor = 0.1; // factor used for stroke and shadow

			string fsText = "font-size: " + MakeCssValue(formatting.MainText.Size * fontFactor * factor, "pt") + ";";
			string strokeText = MakeCssValue(formatting.MainText.Size * fontFactor * factor * strokeFactor, "px");


			string lhText = GetLineHeight(song);

			string mgTextBottom = "padding-bottom: " + MakeCssValue(formatting.BorderBottom * mgFactor * factor, "px") + ";";
			string mgTextTop = "padding-top: " + MakeCssValue(formatting.BorderTop * mgFactor * factor, "px") + ";";
			string mgTextRight = "padding-right: " + MakeCssValue(formatting.BorderRight * mgFactor * factor, "px") + ";";
			string mgCopyRight = "right: " + MakeCssValue(formatting.BorderRight * mgFactor * factor, "px") + ";";
			string mgTextLeft = "padding-left: " + MakeCssValue(formatting.BorderLeft * mgFactor * factor, "px") + ";";
			string mgCopyLeft = "left: " + MakeCssValue(formatting.BorderLeft * mgFactor * factor, "px") + ";";

			if (formatting.HorizontalOrientation == HorizontalTextOrientation.Right || formatting.HorizontalOrientation == HorizontalTextOrientation.Center)
			{
				mgTextLeft = String.Empty;
				mgCopyLeft = String.Empty;
			}
			if (formatting.HorizontalOrientation == HorizontalTextOrientation.Left || formatting.HorizontalOrientation == HorizontalTextOrientation.Center)
			{
				mgTextRight = String.Empty;
				mgCopyRight = String.Empty;
			}
			if (formatting.VerticalOrientation == VerticalTextOrientation.Top || formatting.VerticalOrientation == VerticalTextOrientation.Center)
				mgTextBottom = String.Empty;
			if (formatting.VerticalOrientation == VerticalTextOrientation.Bottom || formatting.VerticalOrientation == VerticalTextOrientation.Center)
				mgTextTop = String.Empty;

			string textFont = "font-family: " + formatting.MainText.Name + ";";
			string textWeight = formatting.MainText.Bold ? "font-weight: bold;" : String.Empty;
			string textFontStyle = formatting.MainText.Italic ? "font-style: italic;" : String.Empty;

			string textColor = "color: " + MakeCssColor(formatting.MainText.Color) + ";";

			string hAlign;

			switch (formatting.HorizontalOrientation)
			{
				default:
				case HorizontalTextOrientation.Left:
					hAlign = "text-align: left;"; break;
				case HorizontalTextOrientation.Center:
					hAlign = "text-align: center;"; break;
				case HorizontalTextOrientation.Right:
					hAlign = "text-align: right;"; break;
			}

			string vAlign;

			switch (formatting.VerticalOrientation)
			{
				default:
				case VerticalTextOrientation.Top:
					vAlign = "vertical-align: top;"; break;
				case VerticalTextOrientation.Center:
					vAlign = "vertical-align: middle;"; break;
				case VerticalTextOrientation.Bottom:
					vAlign = "vertical-align: bottom;"; break;
			}

			string outlineColor = formatting.IsOutlineEnabled ? MakeCssColor(formatting.OutlineColor) : null;
			string shadowColor = formatting.IsShadowEnabled ? MakeCssColor(formatting.ShadowColor) : null;

			string transString = String.Empty;

			if (song.HasTranslation)
			{
				string fsTrans = "font-size: " + MakeCssValue(formatting.TranslationText.Size * fontFactor * factor, "pt") + ";";
				string lhTrans = "height: " + MakeCssValue((formatting.TextLineSpacing + formatting.TranslationText.Size) * lhFactor * factor, "px") + ";";
				string strokeTrans = MakeCssValue(formatting.TranslationText.Size * fontFactor * factor * strokeFactor, "px");

				string transFont = "font-family: " + formatting.TranslationText.Name + ";";
				string transWeight = formatting.TranslationText.Bold ? "font-weight: bold;" : "font-weight: normal;";
				string transFontStyle = formatting.TranslationText.Italic ? "font-style: italic;" : "font-style: normal;";

				string transColor = "color: " + MakeCssColor(formatting.TranslationText.Color) + ";";

				transString = "."+clsMain+" span."+clsTrans+" { " + transFont + transWeight + transFontStyle + transColor + fsTrans + lhTrans + " }";
				transString += @"
 ."+clsBack+" ."+clsTrans+@" {
" + (formatting.IsOutlineEnabled ?
	("-webkit-text-stroke: " + strokeTrans + " " + outlineColor + @";
	text-stroke: " + strokeTrans + " " + outlineColor):"") + @";
" + (formatting.IsShadowEnabled ?
  ("text-shadow: " + shadowColor + " "+strokeTrans+" "+strokeTrans+" "+strokeTrans):"") + @";
 }";
			}

			string fsSource = "font-size: " + MakeCssValue(formatting.SourceText.Size * fontFactor * factor, "pt") + ";";
			string strokeSource = MakeCssValue(formatting.SourceText.Size * fontFactor * factor * strokeFactor, "px");
			string mgSourceTop = "top: " + MakeCssValue(formatting.SourceBorderTop * mgFactor * factor, "px") + ";";
			string mgSourceRight = "right: " + MakeCssValue(formatting.SourceBorderRight * mgFactor * factor, "px") + ";";

			string sourceFont = "font-family: " + formatting.SourceText.Name + ";";
			string sourceWeight = formatting.SourceText.Bold ? "font-weight: bold;" : String.Empty;
			string sourceColor = "color: " + MakeCssColor(formatting.SourceText.Color) + ";";

			string fsCopy = "font-size: " + MakeCssValue(formatting.CopyrightText.Size * fontFactor * factor, "pt") + ";";
			string strokeCopy = MakeCssValue(formatting.CopyrightText.Size * fontFactor * factor * strokeFactor, "px");
			//string lhCopy = "height: "+MakeCssValue(formatting.CopyrightText.Size * fontFactor * factor * lhFactor, "px")+";";
			string mgCopyBottom = "bottom: " + MakeCssValue((formatting.CopyrightBorderBottom + 2) * mgFactor * factor, "px") + ";";

			string copyrightFont = "font-family: " + formatting.CopyrightText.Name + ";";
			string copyrightWeight = formatting.CopyrightText.Bold ? "font-weight: bold;" : String.Empty;
			string copyrightColor = "color: " + MakeCssColor(formatting.CopyrightText.Color) + ";";

			string result = @"body {
 padding: 0;
 margin: 0;
overflow:hidden;
  }
  
  span {
 display: block;
 overflow: visible;
  }

/* Chords */
b {
	font-weight: normal;
	position: relative;
}

b b {
	font-weight: normal;
	font-size: 65%;
	position: absolute;
	top: -50%;
}
  
  ."+clsMain+@" {
 position: absolute;
 width: 100%;
 height: 100%;
 display: table;
table-layout: fixed;
  }

  ."+clsMain+" span { " + lhText + @"}
  
  ."+clsMain+" ."+clsInner+@" {
  " + vAlign + mgTextTop + mgTextBottom + mgTextRight + mgTextLeft + hAlign + fsText + textColor + textFont + textWeight + textFontStyle + @"
  display: table-cell;
  width: 100%;
overflow: hidden;
  }

 " + transString + @"
  
."+clsCopyright+@" {
" + fsCopy + copyrightFont + copyrightColor + copyrightWeight + copyrightWeight + mgCopyBottom + hAlign + mgCopyRight + mgCopyLeft + @"
 position:absolute;
 width: 100%;
}

."+clsSource+@" {
" + mgSourceTop + mgSourceRight + fsSource + sourceFont + sourceWeight + sourceColor + @"
 position: absolute;
}
  
	."+clsBack+" ."+clsMain+@" {
" + (formatting.IsOutlineEnabled ?
 ("-webkit-text-stroke: "+strokeText+" " + outlineColor + @";
 text-stroke: "+strokeText+" " + outlineColor): "") + @";
" + (formatting.IsShadowEnabled ?
  ("text-shadow: " + shadowColor + " "+strokeText+" "+strokeText+" "+strokeText):"") + @";
}
  
  ."+clsBack+" ."+clsSource+@" {
 " + (formatting.IsOutlineEnabled ?
   ("-webkit-text-stroke: "+strokeSource+" " + outlineColor + @";
 text-stroke: "+strokeSource+" " + outlineColor): "") + @";
 " + (formatting.IsShadowEnabled ?
   ("text-shadow: " + shadowColor + " "+strokeSource+" "+strokeSource+" "+strokeSource) : "") + @";
  }

."+clsBack+" ."+clsCopyright+@" {
" + (formatting.IsOutlineEnabled ?
 ("-webkit-text-stroke: "+strokeCopy+" " + outlineColor + @";
 text-stroke: "+strokeCopy+" " + outlineColor): "") + @";
" + (formatting.IsShadowEnabled ?
  ("text-shadow: " + shadowColor + " "+strokeCopy+" "+strokeCopy+" "+strokeCopy) : "") + @";
}";
			//using (StreamWriter sw = new StreamWriter("output.css"))
			//	sw.WriteLine(result);

			return result;
		}
	}
}
