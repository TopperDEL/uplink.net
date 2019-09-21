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

        public bool IsDownloadOperation { get; set; }
        public DownloadOperation DownloadOperation { get; set; }

        public BucketContentViewModel _bucketContentViewModel;

        public ICommand DownloadObjectCommand { get; private set; }
        public ICommand DeleteObjectCommand { get; private set; }
        public ICommand CancelUploadCommand { get; private set; }

        #region Hacks
        //TODO
        //The following properties are only implemented to work on android.
        //There seems to be an issue refreshing "nested" properties.
        public float UploadPercentage { get
            {
                if (UploadOperation != null)
                    return UploadOperation.PercentageCompleted;
                else
                    return 0;
            }
        }

        public bool UploadRunning
        {
            get
            {
                if (UploadOperation != null)
                    return UploadOperation.Running;
                else
                    return false;
            }
        }

        public bool UploadFailed
        {
            get
            {
                if (UploadOperation != null)
                    return UploadOperation.Failed;
                else
                    return false;
            }
        }

        public bool DownloadComplete
        {
            get
            {
                if (DownloadOperation != null)
                    return DownloadOperation.Completed;
                else
                    return false;
            }
        }

        public bool DownloadFailed
        {
            get
            {
                if (DownloadOperation != null)
                    return DownloadOperation.Failed;
                else
                    return false;
            }
        }

        public float DownloadPercentage
        {
            get
            {
                if (DownloadOperation != null)
                    return DownloadOperation.PercentageCompleted;
                else
                    return 0;
            }
        }

        public bool DownloadRunning
        {
            get
            {
                if (DownloadOperation != null)
                    return DownloadOperation.Running;
                else
                    return false;
            }
        }
        #endregion

        public BucketEntryViewModel(BucketContentViewModel bucketContentViewModel, IBucketService bucketService, IObjectService objectService, IStorjService storjService)
        {
            _bucketContentViewModel = bucketContentViewModel;

            DownloadObjectCommand = new DownloadObjectCommand(bucketContentViewModel, bucketService, objectService, storjService, bucketContentViewModel.BucketName);
            DeleteObjectCommand = new DeleteObjectCommand(bucketService, objectService, storjService);
            CancelUploadCommand = new CancelUploadCommand(bucketService, objectService, storjService);
        }
        public void InitDownloadOperation()
        {
            DownloadOperation.DownloadOperationProgressChanged += DownloadOperation_DownloadOperationProgressChanged;
            DownloadOperation.DownloadOperationEnded += DownloadOperation_DownloadOperationEnded;
        }

        private async void DownloadOperation_DownloadOperationEnded(DownloadOperation downloadOperation)
        {
            DownloadOperation.DownloadOperationProgressChanged -= DownloadOperation_DownloadOperationProgressChanged;
            DownloadOperation.DownloadOperationEnded -= DownloadOperation_DownloadOperationEnded;
            if (downloadOperation.Completed)
            {
                BucketContentViewModel.ActiveDownloadOperations[_bucketContentViewModel.BucketName].Remove(downloadOperation);
            }
            RaiseChanged(nameof(DownloadOperation));
            RaiseChanged(nameof(DownloadPercentage));
            RaiseChanged(nameof(DownloadFailed));
            RaiseChanged(nameof(DownloadComplete));
            RaiseChanged(nameof(DownloadRunning));
        }

        private void DownloadOperation_DownloadOperationProgressChanged(DownloadOperation downloadOperation)
        {
            RaiseChanged(nameof(DownloadPercentage));
            RaiseChanged(nameof(DownloadFailed));
            RaiseChanged(nameof(DownloadComplete));
            RaiseChanged(nameof(DownloadRunning));
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
                RaiseChanged(nameof(UploadPercentage));
                RaiseChanged(nameof(UploadFailed));
                RaiseChanged(nameof(UploadRunning));
            }
        }

        private void UploadOperation_UploadOperationProgressChanged(UploadOperation uploadOperation)
        {
            RaiseChanged(nameof(UploadOperation));
            RaiseChanged(nameof(UploadPercentage));
            RaiseChanged(nameof(UploadFailed));
            RaiseChanged(nameof(UploadRunning));
        }
    }
}
