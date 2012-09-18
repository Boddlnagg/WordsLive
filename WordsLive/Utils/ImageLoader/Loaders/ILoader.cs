using System.IO;

namespace WordsLive.Utils.ImageLoader.Loaders
{
    internal interface ILoader
    {
        Stream Load(object source);
    }
}
