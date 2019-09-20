using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using uplink.NET.Interfaces;
using uplink.NET.Models;
using uplink.NET.Sample.Shared.Commands;
using uplink.NET.Sample.Shared.Interfaces;

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

        public ICommand DeleteObjectCommand { get; private set; }
        public ICommand CancelUploadCommand { get; private set; }

        public BucketEntryViewModel(BucketContentViewModel bucketContentViewModel, IBucketService bucketService, IObjectService objectService, IStorjService storjService)
        {
            _bucketContentViewModel = bucketContentViewModel;

            DeleteObjectCommand = new DeleteObjectCommand(bucketService, objectService, storjService);
            CancelUploadCommand = new CancelUploadCommand(bucketService, objectService, storjService);
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
                await _bucketContentViewModel.RefreshAsync();
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
