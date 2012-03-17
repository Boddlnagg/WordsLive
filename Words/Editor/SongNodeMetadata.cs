using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Words.Core.Songs;
using MonitoredUndo;

namespace Words.Editor
{
	public class SongNodeMetadata : SongNode
	{
		public SongNodeMetadata(SongNodeRoot root, string title, string initValue, Action<string> updateSource) : base(root)
		{
			this.Title = title;
			this.Text = initValue;

			this.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "Text")
				{
					updateSource(this.Text);
				}
			};
		}

		private string text;
		public string Text
		{
			get
			{
				return text;
			}
			set
			{
				EditorDocument.OnChangingTryMerge(this, "Text", text, value);
				text = value;
				OnNotifyPropertyChanged("Text");
			}
		}
	}
}
