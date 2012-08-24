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
		public enum MetadataKind
		{
			Copyright,
			Language,
			Category
		}

		private static string GetTitleFromKind(MetadataKind kind)
		{
			switch(kind)
			{
				case MetadataKind.Copyright:
					return Words.Resources.Resource.eMetadataCopyrightTitle;
				case MetadataKind.Language:
					return Words.Resources.Resource.eMetadataLanguageTitle;
				case MetadataKind.Category:
					return Words.Resources.Resource.eMetadataCategoryTitle;
				default:
					throw new ArgumentException("kind");
			}
		}

		public SongNodeMetadata(SongNodeRoot root, MetadataKind kind, string initValue, Action<string> updateSource) : base(root)
		{
			this.Kind = kind;
			this.Title = GetTitleFromKind(Kind);
			this.Text = initValue;

			this.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "Text")
				{
					updateSource(this.Text);
				}
			};
		}

		public MetadataKind Kind { get; private set; }

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
