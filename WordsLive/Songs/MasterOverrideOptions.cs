/*
 * WordsLive - worship projection software
 * Copyright (c) 2013 Patrick Reisert
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
