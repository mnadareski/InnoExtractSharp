using System.Collections.Generic;
using System.IO;

namespace InnoExtractSharp.Streams
{
    public class FilteredStream : Stream
    {
        private Stream baseStream;
        private List<IFilter> filters;
        private List<int> blockSizes;

        public FilteredStream(Stream baseStream)
        {
            filters = new List<IFilter>();
            blockSizes = new List<int>();
        }

        public void Push(IFilter filter, int size = 0)
        {
            filters.Add(filter);
            blockSizes.Add(size);
        }

        // TODO: What would we put here?
        public void Exceptions()
        {
        }

        #region Stream overrides

        public override bool CanRead => baseStream.CanRead;

        public override bool CanSeek => baseStream.CanSeek;

        public override bool CanWrite => baseStream.CanWrite;

        public override long Length => baseStream.Length;

        public override long Position { get => baseStream.Position; set => baseStream.Position = value; }

        public override void Flush()
        {
            baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return baseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            baseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            baseStream.Write(buffer, offset, count);
        }

        #endregion
    }
}
