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

        public BucketContentViewModel _bucketContentViewModel;

        public BucketEntryViewModel(BucketContentViewModel bucketContentViewModel)
        {
            _bucketContentViewModel = bucketContentViewModel;
        }

        public void InitUploadOperation()
        {
            UploadOperation.UploadOperationProgressChanged += UploadOperation_UploadOperationProgressChanged;
            UploadOperation.UploadOperationEnded += UploadOperation_UploadOperationEnded;
        }

        private async void UploadOperation_UploadOperationEnded(UploadOperation uploadOperation)
        {
            UploadOperation.UploadOperationProgressChanged -= UploadOperation_UploadOperationProgressChanged;
            UploadOperation.UploadOperationEnded -= UploadOperation_UploadOperationEnded;
            if(uploadOperation.Completed)
            {
                BucketContentViewModel.ActiveUploadOperations[_bucketContentViewModel.BucketName].Remove(uploadOperation);
                await _bucketContentViewModel.Refresh();
            }
            else
            {
                RaiseChanged(nameof(UploadOperation));
            }
        }

        private void UploadOperation_UploadOperationProgressChanged(UploadOperation uploadOperation)
        {
            RaiseChanged(nameof(UploadOperation));
        }
    }
}
