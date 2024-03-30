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

using System;
using System.Collections.Generic;
using WordsLive.Core;

namespace WordsLive.AudioVideo
{
	public abstract class AudioVideoMedia : Media
	{
		public AudioVideoMedia(Uri uri, Dictionary<string, string> options) : base(uri, options) { }

		public abstract bool HasVideo { get; }

		public override void Load()
		{
			offsetStart = Options.ContainsKey("start") ? new TimeSpan(0, 0, 0, 0, int.Parse(Options["start"])) : new TimeSpan(0);
			offsetEnd = Options.ContainsKey("end") ? new TimeSpan(0, 0, 0, 0, int.Parse(Options["end"])) : new TimeSpan(0);
		}

		private TimeSpan offsetStart;
		private TimeSpan offsetEnd;

		public TimeSpan OffsetStart
		{
			get
			{
				return offsetStart;
			}
			set
			{
				if (value != offsetStart)
				{
					offsetStart = value;
					Options["start"] = ((int)offsetStart.TotalMilliseconds).ToString();
					OnOptionsChanged();
				}
			}
		}

		public TimeSpan OffsetEnd
		{
			get
			{
				return offsetEnd;
			}
			set
			{
				if (value != offsetEnd)
				{
					offsetEnd = value;
					Options["end"] = ((int)offsetEnd.TotalMilliseconds).ToString();
					OnOptionsChanged();
				}
			}
		}
	}
}
