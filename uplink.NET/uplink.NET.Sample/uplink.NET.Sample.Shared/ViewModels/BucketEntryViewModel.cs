using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Models;

namespace uplink.NET.Sample.Shared.ViewModels
{
    public class BucketEntryViewModel : BaseViewModel
    {
        public bool IsUploadOperation { get; set; }
        public UploadOperation UploadOperation { get; set; }

        public bool IsObject { get; set; }
        public ObjectInfo ObjectInfo { get; set; }

        public bool IsDownload { get; set; }
        public DownloadOperation DownloadOperation { get; set; }

        public float PercentageComplete { get; set; }

        public void InitUploadOperation()
        {
            UploadOperation.UploadOperationProgressChanged += UploadOperation_UploadOperationProgressChanged;
        }

        private void UploadOperation_UploadOperationProgressChanged(UploadOperation uploadOperation)
        {
            PercentageComplete = (float)UploadOperation.BytesSent / (float)UploadOperation.TotalBytes * 100f;
            RaiseChanged(nameof(PercentageComplete));
            //RaiseChanged(nameof(UploadOperation));
        }
    }
}
