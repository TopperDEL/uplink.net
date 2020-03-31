using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Interfaces;

namespace uplink.NET.Models
{
    public class DownloadStream : Stream
    {
        private Bucket _bucket;
        private SWIG.DownloadResult _downloadResult;
        private DownloadOperation _download;
        private Scope _scope;
        private string _objectName;
        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        private long _length;
        public override long Length => _length;

        public override long Position { get; set; }

        public DownloadStream(Bucket bucket, int totalBytes, string objectName, Scope scope) //TODO: better Scope-handling
        {
            string error;

            _length = totalBytes;
            _bucket = bucket;
            _objectName = objectName;
            _scope = scope;

            _downloadResult = SWIG.storj_uplink.download_object(_scope.Project, bucket.Name, objectName, null); //TODO: make DownloadOptions available to caller
            _download = new DownloadOperation(_downloadResult, totalBytes, objectName);
            if (!_download.Running && !_download.Completed && !_download.Cancelled && !_download.Failed)
                _download.StartDownloadAsync();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            while ((long)_download.BytesReceived < Position + count)
                Task.Delay(100);

            int available = (int)_download.BytesReceived - offset;
            if (available > count)
                available = count;

            Buffer.BlockCopy(_download.DownloadedBytes, (int)Position, buffer, offset, (int)available);
            return available;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.End:
                    Position = Length + offset;
                    break;
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
            }
            return Position;
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
        }
    }
}
