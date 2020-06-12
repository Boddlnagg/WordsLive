/*
 * WordsLive - worship projection software
 * Copyright (c) 2020 Patrick Reisert
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
using WordsLive.Core.Songs;
using Newtonsoft.Json;

namespace WordsLive.Songs
{
    class SongDisplayBridge
    {
        public event Action CallbackLoaded;

        private SongDisplayController.FeatureLevel features;
        public Song Song { get; set; }
        public bool ShowChords { get; set; }
        public IEnumerable<string> PreloadImages { get; set; }

        public SongDisplayBridge(SongDisplayController.FeatureLevel features)
        {
            this.features = features;
        }

        public string GetFeatureLevel()
        {
            return JsonConvert.SerializeObject(features);
        }

        public string GetBackgroundPrefix()
        {
            return "asset://backgrounds/";
        }

        public string GetSongString()
        {
            return JsonConvert.SerializeObject(Song);
        }

        public bool GetShowChords()
        {
            return ShowChords;
        }

        public string GetPreloadImages()
        {
            if (PreloadImages == null)
                return "[]";
            else
                return JsonConvert.SerializeObject(PreloadImages);
        }

        public void OnCallbackLoaded()
        {
            if (CallbackLoaded != null)
                CallbackLoaded();
        }
    }
}
