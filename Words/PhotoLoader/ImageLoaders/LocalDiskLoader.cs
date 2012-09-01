using System.IO;

namespace Words.PhotoLoader.ImageLoaders
{
    internal class LocalDiskLoader: ILoader
    {
        public Stream Load(string source)
        {
            //Thread.Sleep(1000);
            return File.OpenRead(source);
        }
    }
}
