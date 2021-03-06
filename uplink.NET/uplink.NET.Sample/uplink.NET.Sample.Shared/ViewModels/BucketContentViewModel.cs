﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Interfaces;
using uplink.NET.Models;
using uplink.NET.Sample.Shared.Interfaces;
using System.Linq;
using System.Windows.Input;
using uplink.NET.Sample.Shared.Commands;

namespace uplink.NET.Sample.Shared.ViewModels
{
    public class BucketContentViewModel : BaseViewModel
    {
        public static Dictionary<string, List<UploadOperation>> ActiveUploadOperations = new Dictionary<string, List<UploadOperation>>();
        public static Dictionary<string, List<DownloadOperation>> ActiveDownloadOperations = new Dictionary<string, List<DownloadOperation>>();
        public ObservableCollection<BucketEntryViewModel> Entries { get; set; }
        public string BucketName { get; private set; }
        public ICommand GoBackCommand { get; set; }
        public ICommand UploadFileCommand { get; set; }

        IObjectService _objectService;
        IBucketService _bucketService;
        ILoginService _loginService;

        public BucketContentViewModel(IObjectService objectService, IBucketService bucketService, ILoginService loginService)
        {
            Entries = new ObservableCollection<BucketEntryViewModel>();

            _objectService = objectService;
            _bucketService = bucketService;
            _loginService = loginService;

            GoBackCommand = new GoBackCommand();
            UploadFileCommand = new UploadFileCommand(this, _objectService, _bucketService, _loginService);
        }

        public void SetBucketName(string bucketName)
        {
            BucketName = bucketName;
            ((UploadFileCommand)UploadFileCommand).BucketName = BucketName;
            RaiseChanged(nameof(BucketName));
        }

        public void AddUploadOperation(UploadOperation uploadOperation)
        {
            var entry = new BucketEntryViewModel(this, _bucketService, _objectService);
            entry.IsUploadOperation = true;
            entry.UploadOperation = uploadOperation;
            entry.InitUploadOperation();
            Entries.Add(entry);
        }

        public async Task RemoveDownloadOperationAsync(DownloadOperation downloadOperation)
        {
            var entry = Entries.Where(e => e.DownloadOperation == downloadOperation).FirstOrDefault();
            if (entry != null)
                Entries.Remove(entry);
            else
                await RefreshAsync();
        }

        public void AddDownloadOperation(DownloadOperation downloadOperation)
        {
            var entry = new BucketEntryViewModel(this, _bucketService, _objectService);
            entry.IsDownloadOperation = true;
            entry.DownloadOperation = downloadOperation;
            entry.InitDownloadOperation();
            Entries.Add(entry);
        }

        public async Task RefreshAsync()
        {
            await InvokeAsync(async () =>
            {
                Entries.Clear();
                await InitAsync();
            });
        }

        public async Task InitAsync()
        {
            StartLoading();

            //Fetch all UploadOperations
            var uploadOperations = (ActiveUploadOperations.Where(u => u.Key == BucketName)).FirstOrDefault();
            if (uploadOperations.Value != null)
            {
                foreach (var uploadOperation in uploadOperations.Value)
                {
                    if (!uploadOperation.Completed)
                    {
                        AddUploadOperation(uploadOperation);
                    }
                }
            }

            //Fetch all DownloadOperations
            var downloadOperations = (ActiveDownloadOperations.Where(u => u.Key == BucketName)).FirstOrDefault();
            if (downloadOperations.Value != null)
            {
                foreach (var downloadOperation in downloadOperations.Value)
                {
                    if (!downloadOperation.Completed)
                    {
                        AddDownloadOperation(downloadOperation);
                    }
                }
            }

            //Load all objects
            try
            {
                var bucket = await _bucketService.GetBucketAsync(BucketName);
                var listOptions = new ListObjectsOptions();
                var objects = await _objectService.ListObjectsAsync(bucket, listOptions);
                foreach (var obj in objects.Items)
                {
                    var entry = new BucketEntryViewModel(this, _bucketService, _objectService);
                    entry.IsObject = true;
                    entry.ObjectInfo = obj;
                    Entries.Add(entry);
                }
            }
            catch(Exception ex)
            {
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("Could not open bucket - " + ex.Message);
                await dialog.ShowAsync();
            }

            DoneLoading();
        }
    }
}
