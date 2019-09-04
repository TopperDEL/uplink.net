using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace uplink.NET.Models
{
    /// <summary>
    /// Gets raised to inform about a change within the download-operation progress
    /// </summary>
    /// <param name="downloadOperation">The DownloadOperation that changed</param>
    public delegate void DownloadOperationProgressChanged(DownloadOperation downloadOperation);

    /// <summary>
    /// A DownloadOperation handles a file download in background and informs about progress changes.
    /// </summary>
    public class DownloadOperation : IDisposable
    {
        private readonly byte[] _bytesToDownload;
        /// <summary>
        /// The downloaded bytes - get's filled while the download progresses.
        /// </summary>
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

        /// <summary>
        /// Informs about download-operation progress changes
        /// </summary>
        public event DownloadOperationProgressChanged DownloadOperationProgressChanged;
        /// <summary>
        /// The - until now - received bytes
        /// </summary>
        public ulong BytesReceived { get; private set; }
        /// <summary>
        /// The total bytes to download
        /// </summary>
        public ulong TotalBytes { get; private set; }
        /// <summary>
        /// Is the download completed?
        /// </summary>
        public bool Completed { get; private set; }
        /// <summary>
        /// Did the download fail? See ErrorMessage for details.
        /// </summary>
        public bool Failed { get; set; }
        /// <summary>
        /// Got the download cancelled (by the user)?
        /// </summary>
        public bool Cancelled { get; set; }
        private string _errorMessage;
        /// <summary>
        /// The possible error - only filled if "Failed" is true
        /// </summary>
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

        /// <summary>
        /// Starts the download if it is not yet running, completed, cancelled or failed.
        /// </summary>
        /// <returns></returns>
        public Task StartDownloadAsync()
        {
            if (Completed || Failed || Cancelled)
                return null;

            if (_downloadTask == null)
                _downloadTask = Task.Run(DoDownload);
            return _downloadTask;
        }

        /// <summary>
        /// Cancelles the download progress
        /// </summary>
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

        public void Dispose()
        {
            if(_downloaderRef != null)
            {
                SWIG.storj_uplink.free_downloader(_downloaderRef);
                _downloaderRef = null;
            }
        }
    }
}
