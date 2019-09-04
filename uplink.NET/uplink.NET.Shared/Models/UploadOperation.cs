using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace uplink.NET.Models
{
    /// <summary>
    /// Gets raised to inform about a change within the upload-operation progress
    /// </summary>
    /// <param name="uploadOperation">The UploadOperation that changed</param>
    public delegate void UploadOperationProgressChanged(UploadOperation uploadOperation);
    public class UploadOperation : IDisposable
    {
        private byte[] _bytesToUpload;
        private SWIG.UploaderRef _uploaderRef;
        private Task _uploadTask;
        private bool _cancelled;

        /// <summary>
        /// Informs about upload-operation progress changes
        /// </summary>
        public event UploadOperationProgressChanged UploadOperationProgressChanged;
        /// <summary>
        /// The - until now - sent bytes
        /// </summary>
        public ulong BytesSent { get; private set; }
        /// <summary>
        /// The total bytes to send
        /// </summary>
        public ulong TotalBytes
        {
            get
            {
                if (_bytesToUpload != null)
                    return (ulong)_bytesToUpload.Length;
                else
                    return 0;
            }
        }
        /// <summary>
        /// Is the upload completed?
        /// </summary>
        public bool Completed { get; private set; }
        /// <summary>
        /// Did the upload fail? See ErrorMessage for details.
        /// </summary>
        public bool Failed { get; set; }
        /// <summary>
        /// Got the upload cancelled (by the user)?
        /// </summary>
        public bool Cancelled { get; set; }
        private string _errorMessage;
        /// <summary>
        /// The possible error - only filled if "Failed" is true.
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
        }

        internal UploadOperation(byte[] bytestoUpload, SWIG.UploaderRef uploaderRef)
        {
            _bytesToUpload = bytestoUpload;
            _uploaderRef = uploaderRef;
        }

        /// <summary>
        /// Starts the upload if it is not yet running, completed, cancelled or failed
        /// </summary>
        /// <returns></returns>
        public Task StartUploadAsync()
        {
            if(Completed || Failed || Cancelled)
                return null;

            if (_uploadTask == null)
                _uploadTask = Task.Run(DoUpload);
            return _uploadTask;
        }

        /// <summary>
        /// Cancelles the upload progress
        /// </summary>
        public void Cancel()
        {
            _cancelled = true;
        }

        private void DoUpload()
        {
            if (_bytesToUpload != null)
            {
                while (BytesSent < (ulong)_bytesToUpload.Length)
                {
                    if ((ulong)_bytesToUpload.Length - BytesSent > 1024)
                    {
                        //Send 1024 bytes in next batch
                        var sent = SWIG.storj_uplink.upload_write(_uploaderRef, _bytesToUpload.Skip((int)BytesSent).Take(1024).ToArray(), 1024, out _errorMessage);
                        if (sent != 1024 && string.IsNullOrEmpty(_errorMessage))
                            continue; //try again?
                        if (sent == 1024)
                            BytesSent += 1024;
                    }
                    else
                    {
                        //Send only the remaining bytes
                        var remaining = (ulong)_bytesToUpload.Length - BytesSent;
                        var sent = SWIG.storj_uplink.upload_write(_uploaderRef, _bytesToUpload.Skip((int)BytesSent).Take((int)remaining).ToArray(), (uint)remaining, out _errorMessage);
                        if (sent != remaining && string.IsNullOrEmpty(_errorMessage))
                            continue; //try again?
                        if (sent == remaining)
                            BytesSent += remaining;
                    }
                    if (_cancelled)
                    {
                        SWIG.storj_uplink.upload_cancel(_uploaderRef, out _errorMessage);
                        if (string.IsNullOrEmpty(_errorMessage))
                            Cancelled = true;
                        else
                            Failed = true;
                        return;
                    }
                    UploadOperationProgressChanged?.Invoke(this);
                    if (!string.IsNullOrEmpty(_errorMessage))
                    {
                        Failed = true;
                        return;
                    }
                }
                SWIG.storj_uplink.upload_commit(_uploaderRef, out _errorMessage);
            }
            if (!string.IsNullOrEmpty(_errorMessage))
            {
                Failed = true;
                return;
            }

            Completed = true;
        }

        public void Dispose()
        {
            if(_uploaderRef!= null)
            {
                SWIG.storj_uplink.free_uploader(_uploaderRef);
                _uploaderRef = null;
            }
        }
    }
}
