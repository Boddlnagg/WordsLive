using System.IO;

namespace Words.Utils.ImageLoader.Loaders
{
    internal interface ILoader
    {
        Stream Load(object source);
    }
}
