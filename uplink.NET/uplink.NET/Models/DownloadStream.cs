using System;
using System.Buffers;
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
        private SWIG.UplinkDownloadOptions _options;
        private SWIG.UplinkDownloadResult _result;
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
            _options = new SWIG.UplinkDownloadOptions { length = totalBytes, offset = Position };
            _result = SWIG.storj_uplink.uplink_download_object(_bucket._projectRef, _bucket.Name, _objectName, _options);
        }

        public override void Flush()
        {
        }

        public unsafe override int Read(byte[] buffer, int offset, int count)
        {
            int readBytes = 0;
            int remaining = count;
            var shared = ArrayPool<byte>.Shared;

            byte[] tmpBuffer = shared.Rent(count);
            try
            {
                fixed (byte* arrayPtr = tmpBuffer)
                {
                    while (readBytes < count)
                    {
                        using (SWIG.UplinkReadResult readResult = SWIG.storj_uplink.uplink_download_read(_result.download, new SWIG.SWIGTYPE_p_void(new IntPtr(arrayPtr), true), (uint)remaining))
                        {
                            Array.Copy(tmpBuffer, 0, buffer, readBytes, (int)readResult.bytes_read);
                            remaining -= (int)readResult.bytes_read;
                            Position += (int)readResult.bytes_read;
                            readBytes += (int)readResult.bytes_read;
                            if (readResult.error != null && readResult.error.code == -1)
                            {
                                return readBytes;
                            }
                        }
                    }
                    return readBytes;
                }
            }
            finally
            {
                shared.Return(tmpBuffer);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            _options.Dispose();
            _result.Dispose();

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

            _options = new SWIG.UplinkDownloadOptions { length = _length, offset = Position };
            _result = SWIG.storj_uplink.uplink_download_object(_bucket._projectRef, _bucket.Name, _objectName, _options);

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
            if (_options != null)
            {
                _options.Dispose();
                _options = null;
            }
            if (_result != null)
            {
                _result.Dispose();
                _result = null;
            }
        }
    }
}
