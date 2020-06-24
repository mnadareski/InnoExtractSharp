using System.IO;

namespace InnoExtractSharp.Streams
{
    public interface IFilter
    {
        int Read(Stream src, byte[] dest, int offset, int n);
    }
}
