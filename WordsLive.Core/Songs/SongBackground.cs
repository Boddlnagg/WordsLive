using System.Drawing;

namespace Words.Core.Songs
{
    public class SongBackground
    {
        public bool IsImage
        {
            get
            {
                return (!string.IsNullOrEmpty(ImagePath));
            }
        }

        public string ImagePath { get; set; }

        public Color Color { get; set; }

        public SongBackground()
        {
            ImagePath = "";
            Color = Color.Black;
        }

        public override bool Equals(object obj)
        {
            SongBackground bg = obj as SongBackground;
            if (bg == null)
                return false;

            if (this.IsImage)
            {
                return bg.IsImage && bg.ImagePath == this.ImagePath;
            }
            else
            {
                return !bg.IsImage && bg.Color == this.Color;
            }
        }

        public override int GetHashCode()
        {
            if (this.IsImage)
                return IsImage.GetHashCode() ^ this.ImagePath.GetHashCode();
            else
                return IsImage.GetHashCode() ^ this.Color.GetHashCode();
        }
    }
}