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
        private readonly Bucket _bucket;
        private readonly string _objectName;
        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        private readonly long _length;
        public override long Length => _length;

        public override long Position { get; set; }

        public DownloadStream(Bucket bucket, int totalBytes, string objectName) 
        {
            _length = totalBytes;
            _bucket = bucket;
            _objectName = objectName;
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int received = 0;

            using (var downloadResult = SWIG.storj_uplink.uplink_download_object(_bucket._projectRef, _bucket.Name, _objectName, new SWIG.UplinkDownloadOptions { length = count, offset = Position }))
            {

                if (downloadResult.error != null && !string.IsNullOrEmpty(downloadResult.error.message))
                    throw new EndOfStreamException(downloadResult.error.message);

                using (var download = new DownloadOperation(downloadResult, count, _objectName))
                {
                    download.StartDownloadAsync().Wait();

                    Buffer.BlockCopy(download.DownloadedBytes, 0, buffer, offset, (int)download.BytesReceived);

                    received = (int)download.BytesReceived;
                }

                return received;
            }
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
                default:
                    throw new NotSupportedException();
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
    }
}
