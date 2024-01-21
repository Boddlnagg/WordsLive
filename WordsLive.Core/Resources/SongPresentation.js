// constants
SongPresentation.Transition = 1000;
SongPresentation.FontFactor = 1.3;
SongPresentation.MarginFactor = 1.3;
SongPresentation.LineFactor = 1.28;
SongPresentation.BackgroundPrefix = "";

SongPresentation.FeatureLevel = {
	None: 0,
	Backgrounds: 1,
	Transitions: 2
}

// enums
SongPresentation.HorizontalTextOrientation = {
	Left: 0,
	Center: 1,
	Right: 2
}

SongPresentation.VerticalTextOrientation = {
	Top: 0,
	Center: 1,
	Bottom: 2
}

SongPresentation.MetadataDisplayPosition = {
	None: 0,
	FirstSlide: 1,
	LastSlide: 2,
	AllSlides: 3
}

SongPresentation.TranslationPosition = {
	Inline: 0,
	Block: 1
}

// constructor
function SongPresentation(id, song, featureLevel) {
	// create general styles when they don't yet exist
	if ($('#song-style-general').length == 0)
		SongPresentation.insertStyleGeneral();

	this.id = id;
	this.song = song;
	this.slide = null;
	this.isShown = false;
	this.showChords = true;
	this.container = $('<div>')
		.addClass("presentation")
		.addClass("song")
		.attr('id', 'presentation_' + this.id)
		.hide()
		.append(SongPresentation.createContainer());

	$('head').append($('<style>').attr('id', 'song-style-presentation_' + this.id).append(this.generateStyle()));

	this.backgroundsEnabled = false;
	this.transitionsEnabled = false;

	if (featureLevel >= SongPresentation.FeatureLevel.Backgrounds) {
		this.backgroundsEnabled = true;

		if (this.song.VideoBackground !== null) {
			this.container.prepend($('<video src="' + SongPresentation.BackgroundPrefix + this.song.VideoBackground.Video + '" style="width: 100%; background-color: black;" autoplay="autoplay" muted="muted" loop="loop" >').addClass('song-background'));
		}

		this.container.find('.song-background').show();

		if (featureLevel >= SongPresentation.FeatureLevel.Transitions) {
			this.transitionsEnabled = true;
		}
	}

	this.setCopyright(song.Copyright);
	this.setSource(song.Sources[0]);

	var ths = this;

	$(window).bind('resize.presentation_' + this.id, function () {
		ths.container.find('.song-main > div > div > div').css('font-size', (ths.slide.Size * SongPresentation.FontFactor * ths.getFactor()) + "pt");
		ths.updateStyle();
	});
}

SongPresentation.prototype.updateStyle = function () {
	$('head style#song-style-presentation_' + this.id).html(this.generateStyle());
}

SongPresentation.prototype.destroy = function () {
	// unbind resize event
	$(window).unbind('resize.presentation_' + this.id);
	// remove presentation-specific style
	$('head style#song-style-presentation_' + this.id).remove();
}

SongPresentation.prototype.getContainer = function () {
	return this.container;
}

SongPresentation.prototype.getFactor = function () {
	return window.innerWidth / 1024;
}

SongPresentation.prototype.setShowChords = function (value) {
	if (value != this.showChords) {
		this.showChords = value;
		this.updateStyle();
		if (this.slide != null)
			this.showSlide(this.slide);
	}
}

SongPresentation.prototype.updateFormatting = function (formatting, hasTranslation, hasChords) {
	this.song.Formatting = formatting;
	this.song.HasTranslation = hasTranslation;
	this.song.HasChords = hasChords;
	this.updateStyle();
}

SongPresentation.prototype.setSource = function (source) {
	this.container.find('.song-source > div > div').replaceWith(function () { return $('<div>').append(source.replaceAll(' ', '\u00A0')); });
}

SongPresentation.prototype.setCopyright = function (copyright) {
	var spans = $.map(copyright.split('\n'), function (line, i) {
		return $('<span>').append(line.replaceAll(' ', '\u00A0'));
	});

	var div = $('<div>').append(spans);
	this.container.find('.song-copyright > div > div').replaceWith(function () { return div.clone(); });
}

SongPresentation.prototype.showSource = function (show) {
	if (show)
		this.container.find('.song-source').show();
	else
		this.container.find('.song-source').hide();
}

SongPresentation.prototype.showCopyright = function (show) {
	if (show)
		this.container.find('.song-copyright').show();
	else
		this.container.find('.song-copyright').hide();
}

SongPresentation.prototype.gotoSlide = function (partIndex, slideIndex) {

	var part = this.song.Parts[this.song.Order[partIndex]];
	var slide = part.Slides[slideIndex];

	var showSource = ((this.song.Formatting.SourceDisplayPosition == SongPresentation.MetadataDisplayPosition.AllSlides ||
		(partIndex == 0 && slideIndex == 0 && this.song.Formatting.SourceDisplayPosition == SongPresentation.MetadataDisplayPosition.FirstSlide) ||
		(partIndex == this.song.Order.length - 1 && slideIndex == part.Slides.length - 1 && this.song.Formatting.SourceDisplayPosition == SongPresentation.MetadataDisplayPosition.LastSlide)));

	var showCopyright = ((this.song.Formatting.CopyrightDisplayPosition == SongPresentation.MetadataDisplayPosition.AllSlides ||
		(partIndex == 0 && slideIndex == 0 && this.song.Formatting.CopyrightDisplayPosition == SongPresentation.MetadataDisplayPosition.FirstSlide) ||
		(partIndex == this.song.Order.length - 1 && slideIndex == part.Slides.length - 1 && this.song.Formatting.CopyrightDisplayPosition == SongPresentation.MetadataDisplayPosition.LastSlide)));

	this.showSlide({
		Text: slide.Text,
		Translation: slide.Translation,
		Size: slide.Size,
		Background: this.song.Backgrounds[slide.BackgroundIndex],
		Source: showSource,
		Copyright: showCopyright
	});
}

SongPresentation.prototype.gotoBlankSlide = function (background) {
	this.showSlide({
		Text: "",
		Translation: "",
		Size: this.song.Formatting.MainText.Size,
		Background: background,
		Source: false,
		Copyright: false
	});
}

SongPresentation.prototype.showSlide = function (slide) {
	if (slide === undefined || slide === null)
		throw "Argument null: slide";

	this.slide = slide;

	var inner = $('<div>').css('font-size', (slide.Size * SongPresentation.FontFactor * this.getFactor()) + "pt");

	var lines = slide.Text.split('\n');

	if (slide.Translation) {
		inner.addClass('song-translated');
		var transLines = slide.Translation.split('\n');
		var lineCount = lines.length >= transLines.length ? lines.length : transLines.length;
		var i;
		for (i = 0; i < lineCount; i++) {
			var textLine;
			var transLine;

			if (i < lines.length)
				textLine = lines[i];
			else
				textLine = '';

			if (i < transLines.length)
				transLine = transLines[i];
			else
				transLine = '';

			inner.append($('<span>').append(this.parseLine(textLine, true)));
			inner.append($('<span>').addClass('song-trans').append(this.parseLine(transLine, true, true)));
		}
	} else {
		for (i = 0; i < lines.length; i++) {
			inner.append($('<span>').append(this.parseLine(lines[i], true)));
		}
	}

	var bgCss = null;
	if (this.backgroundsEnabled && slide.Background != null) {
		if (slide.Background.Color !== undefined) {
			bgCss = { 'background-color': SongPresentation.makeCssColor(slide.Background.Color), 'background-image': 'none' };
		} else if (slide.Background.Image !== undefined) {
			bgCss = { 'background-color': 'black', 'background-image': 'url(\'' + SongPresentation.BackgroundPrefix + slide.Background.Image + '\')' };
		} else {
			// if it's a video, remove any background css
			bgCss = { 'background-color': 'none', 'background-image': 'none' };
		}
	}

	if (!this.transitionsEnabled || !this.isShown) {
		this.container.find('.song-main > div > div').replaceWith(function () { return $('<div>').append(inner.clone()); });
		if (bgCss !== null)
			this.container.find('.song-background').css(bgCss);

		if (slide.Source)
			this.container.find('.song-source').show();
		else
			this.container.find('.song-source').hide();
		if (slide.Copyright)
			this.container.find('.song-copyright').show();
		else
			this.container.find('.song-copyright').hide();
	} else {
		var oldMain = this.container.find('.song-current.song-main');
		oldMain.after($('<div>').hide().addClass('song-next').addClass('song-main').append(SongPresentation.createLayer()));

		this.container.find('.song-next.song-main > div > div').replaceWith(function () { return $('<div>').append(inner.clone()); });
		if (bgCss !== null)
			oldMain.after($('<div>').hide().addClass('song-next').addClass('song-background').css(bgCss));

		var old = this.container.find('.song-current');

		this.container.find('.song-next').fadeIn(SongPresentation.Transition, function () { old.remove(); $(this).removeClass('song-next').addClass('song-current'); });
		if (this.song.VideoBackground !== null) {
			this.container.find('.song-current.song-main').fadeOut(SongPresentation.Transition);
		}

		if (slide.Source)
			this.container.find('.song-source').fadeIn(SongPresentation.Transition);
		else
			this.container.find('.song-source').fadeOut(SongPresentation.Transition);
		if (slide.Copyright)
			this.container.find('.song-copyright').fadeIn(SongPresentation.Transition);
		else
			this.container.find('.song-copyright').fadeOut(SongPresentation.Transition);
	}

	// Set one top left pixel to a random color (but it's almost invisible due to opacity).
	// This is required to force an repaint (invalidation) even when the content of the slide
	// doesn't otherwise change (but a repaint is required to detect the necessity of a
	// background change when the background is rendered handled outside of CEF)
	var r = Math.floor(Math.random() * 256);
	var g = Math.floor(Math.random() * 256);
	var b = Math.floor(Math.random() * 256);
	this.container.find('.song-randompixel').css('background', "rgb(" + r + "," + g + "," + b + ")");

	this.isShown = true;
}

// parses chords in a line of the song text
SongPresentation.prototype.parseLine = function (line, parseChords, ignoreChords = false) {
	var result = new Array();
	var index = 0; // current index in result array

	if (line) {
		line = '\uFEFF' + line.replaceAll(' ', '\u00A0'); // the \uFEFF (zero-width space) makes sure that the lines starts correcty
	} else {
		return '';
	}

	var start = 0;

	if (parseChords) {
		var rest = line;
		var pos = 0;

		while ((i = rest.indexOf('[')) !== -1) {
			end = rest.indexOf(']', i);
			if (end === -1)
				break;

			next = rest.indexOf('[', i + 1);
			if (next !== -1 && next < end) {
				rest = rest.substring(i + 1);
				pos += i + 1;
				continue;
			}

			found = true;

			pos += i; // absolute position of '[Chord]'
			chordlen = end - (i + 1) + 2; // length of '[Chord]'

			result[index++] = line.substring(start, pos); // append text to result
			if (this.showChords && !ignoreChords) {
				var chord = line.substring(pos + 1, pos + chordlen - 1);

				// abusing the <b> tag for chords for brevity
				// we need two nested tags, the outer one with position:relative,
				// the inner one with position:absolute (see css)
				result[index++] = $('<b>').append($('<b>').append(chord));
			}
			start = pos + chordlen;

			rest = rest.substring(end + 1);
			pos += -i + end + 1;
		}
	}

	result[index++] = line.substring(start);
	return result;
}

SongPresentation.prototype.generateStyle = function () {
	// TODO: use Powerpraise's song settings for stroke and shadow size?
	var formatting = this.song.Formatting;

	if (formatting.TranslationPosition == SongPresentation.TranslationPosition.Block)
		throw "Translation block positioning is not yet supported."; // TODO

	var makeCssValue = function (value, unit) {
		return value + unit;
	};

	var factor = this.getFactor();

	var idselector = '#presentation_' + this.id;

	var chordsHeight = 0;

	if (this.song.HasChords && this.showChords)
		chordsHeight = 0.5 * formatting.MainText.Size;

	var getLineHeight = function (showChords, showTranslation) {
		var value = formatting.MainText.Size;

		if (showTranslation)
			value += formatting.TranslationLineSpacing;
		else
			value += formatting.TextLineSpacing + chordsHeight;

		return "height: " + makeCssValue(value * SongPresentation.LineFactor * factor, "px") + ";";
	};

	var fsText = "font-size: " + makeCssValue(formatting.MainText.Size * SongPresentation.FontFactor * factor, "pt") + ";";
	var strokeText = formatting.MainText.Size * SongPresentation.FontFactor * factor;
	var lhTextWithoutTrans = getLineHeight(this.song.HasChords && this.showChords, false);
	var lhTextWithTrans = getLineHeight(this.song.HasChords && this.showChords, true);

	var mgTextBottom = "padding-bottom: " + makeCssValue(formatting.BorderBottom * SongPresentation.MarginFactor * factor, "px") + ";";
	var mgTextTop = "padding-top: " + makeCssValue(formatting.BorderTop * SongPresentation.MarginFactor * factor, "px") + ";";
	var mgTextRight = "padding-right: " + makeCssValue(formatting.BorderRight * SongPresentation.MarginFactor * factor, "px") + ";";
	var mgCopyRight = "right: " + makeCssValue(formatting.BorderRight * SongPresentation.MarginFactor * factor, "px") + ";";
	var mgTextLeft = "padding-left: " + makeCssValue(formatting.BorderLeft * SongPresentation.MarginFactor * factor, "px") + ";";
	var mgCopyLeft = "left: " + makeCssValue(formatting.BorderLeft * SongPresentation.MarginFactor * factor, "px") + ";";

	if (formatting.HorizontalOrientation == SongPresentation.HorizontalTextOrientation.Right || formatting.HorizontalOrientation == SongPresentation.HorizontalTextOrientation.Center) {
		mgTextLeft = '';
		mgCopyLeft = '';
	}
	if (formatting.HorizontalOrientation == SongPresentation.HorizontalTextOrientation.Left || formatting.HorizontalOrientation == SongPresentation.HorizontalTextOrientation.Center) {
		mgTextRight = '';
		mgCopyRight = '';
	}
	if (formatting.VerticalOrientation == SongPresentation.VerticalTextOrientation.Top || formatting.VerticalOrientation == SongPresentation.VerticalTextOrientation.Center) {
		mgTextBottom = '';
	}
	if (formatting.VerticalOrientation == SongPresentation.VerticalTextOrientation.Bottom || formatting.VerticalOrientation == SongPresentation.VerticalTextOrientation.Center) {
		mgTextTop = '';
	}

	var textFont = "font-family: " + formatting.MainText.Name + ";";
	var textWeight = formatting.MainText.Bold ? "font-weight: bold;" : '';
	var textFontStyle = formatting.MainText.Italic ? "font-style: italic;" : '';

	var textColor = "color: " + SongPresentation.makeCssColor(formatting.MainText.Color) + ";";

	var hAlign;

	switch (formatting.HorizontalOrientation) {
		default:
		case SongPresentation.HorizontalTextOrientation.Left:
			hAlign = "text-align: left;"; break;
		case SongPresentation.HorizontalTextOrientation.Center:
			hAlign = "text-align: center;"; break;
		case SongPresentation.HorizontalTextOrientation.Right:
			hAlign = "text-align: right;"; break;
	}

	var vAlign;

	switch (formatting.VerticalOrientation) {
		default:
		case SongPresentation.VerticalTextOrientation.Top:
			vAlign = "vertical-align: top;"; break;
		case SongPresentation.VerticalTextOrientation.Center:
			vAlign = "vertical-align: middle;"; break;
		case SongPresentation.VerticalTextOrientation.Bottom:
			vAlign = "vertical-align: bottom;"; break;
	}

	var outlineColor = formatting.IsOutlineEnabled ? SongPresentation.makeCssColor(formatting.OutlineColor) : null;
	var shadowColor = formatting.IsShadowEnabled ? SongPresentation.makeCssColor(formatting.ShadowColor) : null;

	var createStrokeShadowRules = function (strokeSize, shadowSize) {
		var result = '';
		var shadowRules = [];

		strokeSize *= 0.1;
		shadowSize *= 0.1;

		if (formatting.IsOutlineEnabled) {
			strokeSizeValue = makeCssValue(strokeSize, 'px');
			result = '-webkit-text-stroke: ' + strokeSizeValue + ' ' + outlineColor + ';\
						  text-stroke: ' + strokeSizeValue + ' ' + outlineColor + ';';
		}

		if (formatting.IsShadowEnabled) {
			shadowSizeValue = makeCssValue(shadowSize, 'px');
			result += 'text-shadow: ' + shadowColor + ' ' + shadowSizeValue + ' ' + shadowSizeValue + ' ' + shadowSizeValue + ';'
		}

		return result;
	}

	var cssTrans = '';

	if (this.song.HasTranslation) {
		var fsTrans = "font-size: " + makeCssValue(formatting.TranslationText.Size * SongPresentation.FontFactor * factor, "pt") + ";";
		var lhTrans = "height: " + makeCssValue((formatting.TextLineSpacing + formatting.TranslationText.Size + chordsHeight) * SongPresentation.LineFactor * factor, "px") + ";";
		var strokeTrans = formatting.TranslationText.Size * SongPresentation.FontFactor * factor;

		var transFont = "font-family: " + formatting.TranslationText.Name + ";";
		var transWeight = formatting.TranslationText.Bold ? "font-weight: bold;" : "font-weight: normal;";
		var transFontStyle = formatting.TranslationText.Italic ? "font-style: italic;" : "font-style: normal;";

		var transColor = "color: " + SongPresentation.makeCssColor(formatting.TranslationText.Color) + ";";

		cssTrans = '\
		' + idselector + ' .song-main span.song-trans {\
			' + transFont + '\
			' + transWeight + '\
			' + transFontStyle + '\
			' + transColor + '\
			' + fsTrans + '\
			' + lhTrans + '\
		}';

		var strokeShadowTrans = createStrokeShadowRules(strokeTrans, strokeTrans);

		if (strokeShadowTrans) {
			cssTrans += idselector + ' .song-back .song-trans {' + strokeShadowTrans + '}';
		}
	}

	var fsSource = "font-size: " + makeCssValue(formatting.SourceText.Size * SongPresentation.FontFactor * factor, "pt") + ";";
	var strokeSource = formatting.SourceText.Size * SongPresentation.FontFactor * factor;
	var mgSourceTop = "top: " + makeCssValue(formatting.SourceBorderTop * SongPresentation.MarginFactor * factor, "px") + ";";
	var mgSourceRight = "right: " + makeCssValue(formatting.SourceBorderRight * SongPresentation.MarginFactor * factor, "px") + ";";

	var sourceFont = "font-family: " + formatting.SourceText.Name + ";";
	var sourceWeight = formatting.SourceText.Bold ? "font-weight: bold;" : '';
	var sourceColor = "color: " + SongPresentation.makeCssColor(formatting.SourceText.Color) + ";";

	var fsCopy = "font-size: " + makeCssValue(formatting.CopyrightText.Size * SongPresentation.FontFactor * factor, "pt") + ";";
	var strokeCopy = formatting.CopyrightText.Size * SongPresentation.FontFactor * factor;
	//var lhCopy = "height: "+makeCssValue(formatting.CopyrightText.Size * SongPresentation.FontFactor * factor * SongPresentation.LineFactor, "px")+";";
	var mgCopyBottom = "bottom: " + makeCssValue((formatting.CopyrightBorderBottom + 2) * SongPresentation.MarginFactor * factor, "px") + ";";

	var copyrightFont = "font-family: " + formatting.CopyrightText.Name + ";";
	var copyrightWeight = formatting.CopyrightText.Bold ? "font-weight: bold;" : '';
	var copyrightColor = "color: " + SongPresentation.makeCssColor(formatting.CopyrightText.Color) + ";";

	var cssSpecific = cssTrans + '\
	' + idselector + ' .song-main span {' + lhTextWithoutTrans + '}\
	' + idselector + ' .song-main .song-translated span:not(.song-trans) {' + lhTextWithTrans + '}\
	\
	' + idselector + ' .song-main > div > div > div {\
		' + vAlign + '\
		' + mgTextTop + '\
		' + mgTextBottom + '\
		' + mgTextRight + '\
		' + mgTextLeft + '\
		' + hAlign + '\
		' + fsText + '\
		' + textColor + '\
		' + textFont + '\
		' + textWeight + '\
		' + textFontStyle + '\
		display: table-cell;\
		width: 100%;\
		overflow: hidden;\
	}\
	\
	' + idselector + ' .song-copyright > div > div {\
		' + fsCopy + '\
		' + copyrightFont + '\
		' + copyrightColor + '\
		' + copyrightWeight + '\
		' + copyrightWeight + '\
		' + mgCopyBottom + '\
		' + hAlign + '\
		' + mgCopyRight + '\
		' + mgCopyLeft + '\
		position:absolute;\
		width: 100%;\
	}\
	\
	' + idselector + ' .song-source > div > div {\
		' + mgSourceTop + '\
		' + mgSourceRight + '\
		' + fsSource + '\
		' + sourceFont + '\
		' + sourceWeight + '\
		' + sourceColor + '\
		position: absolute;\
	}';

	var strokeShadowText = createStrokeShadowRules(strokeText, strokeText);

	if (strokeShadowText) {
		cssSpecific += idselector + ' .song-main .song-back > div {' + strokeShadowText + '}';
	}

	var strokeShadowSource = createStrokeShadowRules(strokeSource, strokeSource);

	if (strokeShadowSource) {
		cssSpecific += idselector + ' .song-source .song-back > div {' + strokeShadowSource + '}';
	}

	var strokeShadowCopyright = createStrokeShadowRules(strokeCopy, strokeCopy);

	if (strokeShadowCopyright) {
		cssSpecific += idselector + ' .song-copyright .song-back > div {' + strokeShadowCopyright + '}';
	}

	return cssSpecific;
}

// static functions
SongPresentation.createLayer = function () {
	// create two layers (for text-stroke)
	return [$('<div>').addClass('song-back').append('<div>'), $('<div>').addClass('song-front').append('<div>')];
}

SongPresentation.createContainer = function () {
	return [$('<div>').hide().addClass('song-current').addClass('song-background'),
			$('<div>').addClass('song-current').addClass('song-main').append(SongPresentation.createLayer()),
			$('<div>').hide().addClass('song-source').append(SongPresentation.createLayer()),
			$('<div>').hide().addClass('song-copyright').append(SongPresentation.createLayer()),
			$('<div>').addClass('song-randompixel')];
}

SongPresentation.makeCssColor = function (value) {
	return 'rgba(' + value[0] + ',' + value[1] + ',' + value[2] + ',' + value[3] + ')';
};

SongPresentation.insertStyleGeneral = function () {
	var cssGeneral = '\
	.song span {                      	\
		display: block;               	\
		overflow: visible;            	\
		white-space: nowrap;            \
	}                                 	\
										\
	/* Chords */                      	\
	.song b {                         	\
		font-weight: normal;          	\
		position: relative;           	\
	}                                 	\
										\
	.song b b {                       	\
		font-weight: normal;          	\
		font-size: 65%;               	\
		position: absolute;           	\
		top: -50%;                    	\
	}                                 	\
										\
	.song-background {                	\
		background-repeat: no-repeat; 	\
		background-size: 100%;        	\
	}                                 	\
										\
	.song-main,                       	\
	.song-background {                	\
		width: 100%;                  	\
		height: 100%;                 	\
		position: absolute;           	\
	}                                 	\
										\
	.song-main > div > div {          	\
		position: absolute;           	\
		width: 100%;                  	\
		height: 100%;                 	\
		display: table;               	\
		table-layout: fixed;          	\
	}                                   \
	.song-randompixel {                 \
		position: absolute;             \
		width: 1px; height: 1px;        \
		opacity: 0.05;                  \
		background-color: white;        \
	}';
	$('head').append($('<style>').attr('id', 'song-style-general').append(cssGeneral));
}