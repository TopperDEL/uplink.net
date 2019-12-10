using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace uplink.NET.Models
{
    public class DownloadStream : Stream
    {
        private BucketRef _bucketRef;
        private string _objectName;
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        private long _length;
        public override long Length => _length;

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public DownloadStream(BucketRef bucketRef, int totalBytes, string objectName)
        {
            _length = totalBytes;
            _bucketRef = bucketRef;
            _objectName = objectName;
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            string error;
            var downloaderRef = SWIG.storj_uplink.download_range(_bucketRef._bucketRef, _objectName, offset, count, out error);

            if (!string.IsNullOrEmpty(error))
                return 0;

            using (var download = new DownloadOperation(downloaderRef, (ulong)count, _objectName))
            {
                var downloadTask = download.StartDownloadAsync();
                downloadTask.Wait();

                Buffer.BlockCopy(download.DownloadedBytes, 0, buffer, 0, (int)download.BytesReceived);
                return (int)download.BytesReceived;
            }
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
        }
    }
}
