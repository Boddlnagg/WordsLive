using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WordsLive.Properties;

namespace WordsLive.Songs
{
	public struct MasterOverrideOptions
	{
		public bool Enable { get; set; }
		public bool TextFormatting { get; set; }
		public bool TextPosition { get; set; }
		public bool SourceFormatting { get; set; }
		public bool SourcePosition { get; set; }
		public bool CopyrightFormatting { get; set; }
		public bool CopyrightPosition { get; set; }
		public bool OutlineShadow { get; set; }

		public static MasterOverrideOptions CreateFromSettings()
		{
			return new MasterOverrideOptions
			{
				Enable = Settings.Default.TemplateMasterEnable,
				TextFormatting = Settings.Default.TemplateMasterOverrideTextFormatting,
				TextPosition = Settings.Default.TemplateMasterOverrideTextPosition,
				SourceFormatting = Settings.Default.TemplateMasterOverrideSourceFormatting,
				SourcePosition = Settings.Default.TemplateMasterOverrideSourcePosition,
				CopyrightFormatting = Settings.Default.TemplateMasterOverrideCopyrightFormatting,
				CopyrightPosition = Settings.Default.TemplateMasterOverrideCopyrightPosition,
				OutlineShadow = Settings.Default.TemplateMasterOverrideOutlineShadow
			};
		}
	}
}
