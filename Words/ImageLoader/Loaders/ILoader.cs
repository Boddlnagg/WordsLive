using System.IO;

namespace Words.ImageLoader.Loaders
{
    internal interface ILoader
    {
        Stream Load(object source);
    }
}
