using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace uplink.NET.Models
{
    public delegate void UploadOperationProgressChanged(UploadOperation uploadOperation);
    public class UploadOperation
    {
        private byte[] _bytesToUpload;
        private SWIG.UploaderRef _uploaderRef;
        private Task _uploadTask;
        private bool _cancelled;

        public event UploadOperationProgressChanged UploadOperationProgressChanged;
        public int BytesSent { get; private set; }
        public int TotalBytes
        {
            get
            {
                if (_bytesToUpload != null)
                    return _bytesToUpload.Length;
                else
                    return 0;
            }
        }
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

        internal UploadOperation(byte[] bytestoUpload, SWIG.UploaderRef uploaderRef)
        {
            _bytesToUpload = bytestoUpload;
            _uploaderRef = uploaderRef;
        }

        public Task StartUploadAsync()
        {
            if (_uploadTask == null)
                _uploadTask = Task.Run(DoUpload);
            return _uploadTask;
        }

        public void Cancel()
        {
            _cancelled = true;
        }

        private void DoUpload()
        {
            if (_bytesToUpload != null)
            {
                while (BytesSent < _bytesToUpload.Length)
                {
                    if(_bytesToUpload.Length - BytesSent > 1024)
                    {
                        //Send 1024 bytes in next batch
                        var sent = SWIG.storj_uplink.upload_write(_uploaderRef, _bytesToUpload.Skip(BytesSent).Take(1024).ToArray(), 1024, out _errorMessage);
                        if (sent != 1024 && string.IsNullOrEmpty(_errorMessage))
                            continue; //try again?
                        if (sent == 1024)
                            BytesSent += 1024;
                    }
                    else
                    {
                        //Send only the remaining bytes
                        var remaining = _bytesToUpload.Length - BytesSent;
                        var sent = SWIG.storj_uplink.upload_write(_uploaderRef, _bytesToUpload.Skip(BytesSent).Take(remaining).ToArray(), (uint)remaining, out _errorMessage);
                        if (sent != remaining && string.IsNullOrEmpty(_errorMessage))
                            continue; //try again?
                        if (sent == remaining)
                            BytesSent += remaining;
                    }
                    if(_cancelled)
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
    }
}
