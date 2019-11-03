using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace uplink.NET.Models
{
    public class DownloadStream : Stream
    {
        private SWIG.DownloaderRef _downloaderRef;
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        private long _length;
        public override long Length => _length;

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private DownloadOperation _download;
        public DownloadStream(SWIG.DownloaderRef downloaderRef, ulong totalBytes, string objectName)
        {
            _length = (long)totalBytes;
            _downloaderRef = downloaderRef;
            _download = new DownloadOperation(downloaderRef, totalBytes, objectName);
            _download.StartDownloadAsync();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            while (_download.BytesReceived < (ulong)offset)
                Task.Delay(100);

            int available = (int)_download.BytesReceived - offset;
            if (available > count)
                available = count;

            Buffer.BlockCopy(_download.DownloadedBytes, offset, buffer, 0, (int)available);
            return available;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _download.Cancel();
            _download.Dispose();
            _download = null;
        }
    }
}
