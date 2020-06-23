using System.IO;

namespace InnoExtractSharp.Streams
{
    public interface IFilter
    {
        int Read(Stream src, ref byte[] dest, int n);
    }
}
