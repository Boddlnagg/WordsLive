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
using WordsLive.Core;

namespace WordsLive
{
	/// <summary>
	/// Attribute used on types implementing IMediaControlPanel to indicate the media type they are created for
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class TargetMediaAttribute : Attribute
	{
		private Type type;

		public Type Type
		{
			get
			{
				return type;
			}
			set
			{
				if (!value.IsSubclassOf(typeof(Media)))
					throw new ArgumentException("Type is not a Media.");

				type = value;
			}
		}

		public TargetMediaAttribute(Type type)
		{
			Type = type;
		}
	}
}
