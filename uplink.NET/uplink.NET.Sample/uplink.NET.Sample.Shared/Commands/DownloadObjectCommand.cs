using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Input;
using uplink.NET.Interfaces;
using uplink.NET.Models;
using uplink.NET.Sample.Shared.Interfaces;
using uplink.NET.Sample.Shared.Pages;
using uplink.NET.Sample.Shared.Services;
using uplink.NET.Sample.Shared.ViewModels;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;

namespace uplink.NET.Sample.Shared.Commands
{
    public class DownloadObjectCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        BucketContentViewModel _senderView;
        IBucketService _bucketService;
        IObjectService _objectService;
        string _bucketName;

        public DownloadObjectCommand(BucketContentViewModel senderView, IBucketService bucketService, IObjectService objectService, string bucketName)
        {
            _senderView = senderView;
            _bucketService = bucketService;
            _objectService = objectService;
            _bucketName = bucketName;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            BucketEntryViewModel bucketEntryVM = parameter as BucketEntryViewModel;
#if!__ANDROID__

            FileSavePicker picker = new FileSavePicker();
            if (bucketEntryVM.ObjectInfo.Path.Contains("mp4"))
            {
                picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
                picker.FileTypeChoices.Add("Video", new List<string>() { ".mp4" });
            }
            else
            {
                picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                picker.FileTypeChoices.Add("Image", new List<string>() { ".jpg" });
            }

            picker.SuggestedFileName = bucketEntryVM.ObjectInfo.Path;


            var file = await picker.PickSaveFileAsync();
            if (file == null)
                return;

            Windows.Storage.CachedFileManager.DeferUpdates(file);

            try
            {
                var bucket = await _bucketService.OpenBucketAsync(bucketEntryVM._bucketContentViewModel.BucketName);
                var downloadOperation = await _objectService.DownloadObjectAsync(bucket, bucketEntryVM.ObjectInfo.Path, true);
                downloadOperation.DownloadOperationEnded += async (operation) =>
                {
                    if (!operation.Failed && !operation.Cancelled)
                    {
                        await Windows.Storage.FileIO.WriteBytesAsync(file, operation.DownloadedBytes);
                        var status = await CachedFileManager.CompleteUpdatesAsync(file);
                    }
                };
                if (BucketContentViewModel.ActiveDownloadOperations.ContainsKey(_bucketName))
                    BucketContentViewModel.ActiveDownloadOperations[_bucketName].Add(downloadOperation);
                else
                {
                    var list = new List<DownloadOperation>();
                    list.Add(downloadOperation);
                    BucketContentViewModel.ActiveDownloadOperations.Add(_bucketName, list);
                }
                _senderView.AddDownloadOperation(downloadOperation);
            }
            catch (Exception ex)
            {
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("Could not download object - " + ex.Message);
                await dialog.ShowAsync();
                return;
            }
#else
            var path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;

            try
            {
                var bucket = await _bucketService.GetBucketAsync(bucketEntryVM._bucketContentViewModel.BucketName);
                var downloadOperation = await _objectService.DownloadObjectAsync(bucket, bucketEntryVM.ObjectInfo.Key, new DownloadOptions(), true);
                downloadOperation.DownloadOperationEnded += async (operation) =>
                {
                    if (!operation.Failed && !operation.Cancelled)
                    {
                        string filePath = Path.Combine(path, "Download");
                        if (!Directory.Exists(filePath))
                            Directory.CreateDirectory(filePath);

                        filePath = Path.Combine(filePath, "Storj");
                        if (!Directory.Exists(filePath))
                            Directory.CreateDirectory(filePath);
                        filePath = Path.Combine(filePath, bucketEntryVM.ObjectInfo.Key);
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            await fs.WriteAsync(operation.DownloadedBytes, 0, (int)operation.TotalBytes);
                        }
                    }
                };
                if (BucketContentViewModel.ActiveDownloadOperations.ContainsKey(_bucketName))
                    BucketContentViewModel.ActiveDownloadOperations[_bucketName].Add(downloadOperation);
                else
                {
                    var list = new List<DownloadOperation>();
                    list.Add(downloadOperation);
                    BucketContentViewModel.ActiveDownloadOperations.Add(_bucketName, list);
                }
                _senderView.AddDownloadOperation(downloadOperation);
            }
            catch (Exception ex)
            {
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("Could not download object - " + ex.Message);
                await dialog.ShowAsync();
                return;
            }
#endif
        }
    }
}
