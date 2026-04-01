using System;
using System.Buffers;
using System.Threading.Tasks;

namespace uplink.NET.Models
{
    /// <summary>
    /// Gets raised to inform about a change within the download-operation progress
    /// </summary>
    /// <param name="downloadOperation">The DownloadOperation that changed</param>
    public delegate void DownloadOperationProgressChanged(DownloadOperation downloadOperation);
    /// <summary>
    /// Gets raised to inform about an ended DownloadOperation.
    /// </summary>
    /// <param name="uploadOperation">The UploadOperation that ended</param>
    public delegate void DownloadOperationEnded(DownloadOperation downloadOperation);

    /// <summary>
    /// A DownloadOperation handles a file download in background and informs about progress changes.
    /// </summary>
    public unsafe class DownloadOperation : IDisposable
    {
        private readonly byte[] _bytesToDownload;
        private readonly SWIG.UplinkDownload _download;
        private SWIG.UplinkDownloadResult _downloadResult;
        private IDisposable _transferLifetime;
        private Task _downloadTask;
        private bool _cancelled;
        private bool _disposed;

        /// <summary>
        /// The downloaded bytes - get's filled while the download progresses.
        /// </summary>
        public byte[] DownloadedBytes
        {
            get
            {
                return _bytesToDownload;
            }
        }
        /// <summary>
        /// The name of the object downloading
        /// </summary>
        public string ObjectName { get; private set; }
        /// <summary>
        /// Informs about download-operation progress changes
        /// </summary>
        public event DownloadOperationProgressChanged DownloadOperationProgressChanged;
        /// <summary>
        /// Inform about a DownloadOperation that ended (i.e. Completed, Failed or got Cancelled)
        /// </summary>
        public event DownloadOperationEnded DownloadOperationEnded;
        /// <summary>
        /// The - until now - received bytes
        /// </summary>
        public long BytesReceived { get; private set; }
        /// <summary>
        /// The total bytes to download
        /// </summary>
        public long TotalBytes { get; private set; }
        /// <summary>
        /// Is the download completed?
        /// </summary>
        public bool Completed { get; private set; }
        /// <summary>
        /// Did the download fail? If true, see ErrorMessage for details.
        /// </summary>
        public bool Failed { get; set; }
        /// <summary>
        /// Got the download cancelled (by the user)?
        /// </summary>
        public bool Cancelled { get; set; }
        /// <summary>
        /// Is the download currently in progress?
        /// </summary>
        public bool Running { get; set; }
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
        /// <summary>
        /// The percentage of completeness
        /// </summary>
        public float PercentageCompleted
        {
            get
            {
                return (float)BytesReceived / (float)TotalBytes * 100f;
            }
        }

        internal DownloadOperation(SWIG.UplinkDownloadResult downloadResult, IDisposable transferLifetime, long totalBytes, string objectName)
        {
            _downloadResult = downloadResult;
            _download = downloadResult.download;
            _transferLifetime = transferLifetime;
            TotalBytes = totalBytes;
            _bytesToDownload = new byte[TotalBytes];
            ObjectName = objectName;
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
            var shared = ArrayPool<byte>.Shared;
            var tenth = _bytesToDownload.Length / 10;
            if (tenth == 0)
                tenth = _bytesToDownload.Length;
            byte[] part = shared.Rent(tenth);

            try
            {
                Running = true;
                while (BytesReceived < TotalBytes)
                {
                    var bytesToRead = TotalBytes - BytesReceived > tenth ? tenth : (int)(TotalBytes - BytesReceived);

                    fixed (byte* arrayPtr = part)
                    {
                        using (SWIG.UplinkReadResult readResult = SWIG.storj_uplink.uplink_download_read(_download, new SWIG.SWIGTYPE_p_void(new IntPtr(arrayPtr), true), (uint)bytesToRead))
                        {
                            if (readResult.error != null && !string.IsNullOrEmpty(readResult.error.message))
                            {
                                _errorMessage = readResult.error.message;
                                Failed = true;
                                return;
                            }
                            if (readResult.bytes_read != 0)
                            {
                                Array.Copy(part, 0, _bytesToDownload, BytesReceived, readResult.bytes_read);
                                BytesReceived += readResult.bytes_read;
                            }
                        }
                    }

                    if (_cancelled)
                    {
                        using (SWIG.UplinkError cancelError = SWIG.storj_uplink.uplink_close_download(_download))
                        {
                            if (cancelError != null && !string.IsNullOrEmpty(cancelError.message))
                            {
                                _errorMessage = cancelError.message;
                                Failed = true;
                            }
                            else
                            {
                                Cancelled = true;
                            }
                        }

                        return;
                    }

                    DownloadOperationProgressChanged?.Invoke(this);
                }

                using (SWIG.UplinkError closeError = SWIG.storj_uplink.uplink_close_download(_download))
                {
                    if (closeError != null && !string.IsNullOrEmpty(closeError.message))
                    {
                        _errorMessage = closeError.message;
                        Failed = true;
                        return;
                    }
                }

                Completed = true;
            }
            finally
            {
                Running = false;
                CleanupNativeResources();
                shared.Return(part);
                DownloadOperationEnded?.Invoke(this);
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (!Completed && !Failed && !Cancelled && _downloadTask != null && !_downloadTask.IsCompleted)
            {
                _cancelled = true;
                return;
            }

            if (!Completed && !Failed && !Cancelled && _download != null)
            {
                using (var closeError = SWIG.storj_uplink.uplink_close_download(_download))
                {
                    if (closeError == null || string.IsNullOrEmpty(closeError.message))
                        Cancelled = true;
                }
            }

            CleanupNativeResources();
        }

        private void CleanupNativeResources()
        {
            if (_downloadResult != null)
            {
                _downloadResult.Dispose();
                _downloadResult = null;
            }

            if (_transferLifetime != null)
            {
                _transferLifetime.Dispose();
                _transferLifetime = null;
            }
        }
    }
}
