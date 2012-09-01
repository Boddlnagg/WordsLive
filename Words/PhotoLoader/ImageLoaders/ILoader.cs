using System.IO;

namespace Words.PhotoLoader.ImageLoaders
{
    internal interface ILoader
    {
        Stream Load(string source);
    }
}
