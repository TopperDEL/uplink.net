using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace uplink.NET.Models
{
    public delegate void DownloadOperationProgressChanged(DownloadOperation downloadOperation);

    public class DownloadOperation
    {
        private byte[] _bytesToDownload;
        public byte[] DownloadedBytes //Maybe a Stream would be better?
        {
            get
            {
                return _bytesToDownload;
            }
        }
        private SWIG.DownloaderRef _downloaderRef;
        private Task _downloadTask;
        private bool _cancelled;

        public event DownloadOperationProgressChanged DownloadOperationProgressChanged;
        public ulong BytesReceived { get; private set; }
        public ulong TotalBytes { get; private set; }
        public bool Completed { get; private set; }
        public bool Failed { get; set; }
        public bool Cancelled { get; set; }
        private string _errorMessage;
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
        }

        internal DownloadOperation(SWIG.DownloaderRef downloaderRef, ulong totalBytes)
        {
            _downloaderRef = downloaderRef;
            TotalBytes = totalBytes;
            _bytesToDownload = new byte[TotalBytes];
        }

        public Task StartDownloadAsync()
        {
            if (_downloadTask == null)
                _downloadTask = Task.Run(DoDownload);
            return _downloadTask;
        }

        public void Cancel()
        {
            _cancelled = true;
        }

        private void DoDownload()
        {
            while (BytesReceived < TotalBytes)
            {
                if (TotalBytes - BytesReceived > 1024)
                {
                    //Fetch 1024 bytes in next batch
                    byte[] part = new byte[1024];
                    var received = SWIG.storj_uplink.download_read(_downloaderRef, part, 1024, out _errorMessage);
                    if (received != 1024 && string.IsNullOrEmpty(_errorMessage))
                        continue; //try again?
                    if (received == 1024)
                    {
                        Array.Copy(part, 0, _bytesToDownload, (long)BytesReceived, 1024);
                        BytesReceived += 1024;
                    }
                }
                else
                {
                    //Fetch only the remaining bytes
                    byte[] part = new byte[1024];

                    var remaining = TotalBytes - BytesReceived;
                    var received = SWIG.storj_uplink.download_read(_downloaderRef, part, (uint)remaining, out _errorMessage);
                    if (received != remaining && string.IsNullOrEmpty(_errorMessage))
                        continue; //try again?
                    if (received == remaining)
                    {
                        Array.Copy(part, 0, _bytesToDownload, (long)BytesReceived, (long)remaining);
                        BytesReceived += remaining;
                    }
                }

                if (_cancelled)
                {
                    SWIG.storj_uplink.download_cancel(_downloaderRef, out _errorMessage);
                    if (string.IsNullOrEmpty(_errorMessage))
                        Cancelled = true;
                    else
                        Failed = true;
                    return;
                }
                DownloadOperationProgressChanged?.Invoke(this);
                if (!string.IsNullOrEmpty(_errorMessage))
                {
                    Failed = true;
                    return;
                }
            }

            SWIG.storj_uplink.download_close(_downloaderRef, out _errorMessage);

            if (!string.IsNullOrEmpty(_errorMessage))
            {
                Failed = true;
                return;
            }

            Completed = true;
        }
    }
}
