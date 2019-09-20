using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using uplink.NET.Interfaces;
using uplink.NET.Sample.Shared.Interfaces;
using uplink.NET.Sample.Shared.Pages;
using uplink.NET.Sample.Shared.Services;
using uplink.NET.Sample.Shared.ViewModels;
using Windows.UI.Xaml.Controls;

namespace uplink.NET.Sample.Shared.Commands
{
    public class DeleteObjectCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        IBucketService _bucketService;
        IObjectService _objectService;
        IStorjService _storjService;

        public DeleteObjectCommand(IBucketService bucketService, IObjectService objectService, IStorjService storjService)
        {
            _bucketService = bucketService;
            _objectService = objectService;
            _storjService = storjService;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            BucketEntryViewModel bucketEntryVM = parameter as BucketEntryViewModel;

            if (bucketEntryVM.IsObject)
            {
                ContentDialog deleteObjectDialog = new ContentDialog
                {
                    Title = "Delete '" + bucketEntryVM.ObjectInfo.Path + "'",
                    Content = "Do you really want to delete the object '" + bucketEntryVM.ObjectInfo.Path + "' ?",
                    CloseButtonText = "No",
                    PrimaryButtonText = "Yes"
                };

                ContentDialogResult result = await deleteObjectDialog.ShowAsync();
                if (result != ContentDialogResult.Primary)
                    return;
                try
                {
                    var bucket = await _bucketService.OpenBucketAsync(_storjService.Project, bucketEntryVM._bucketContentViewModel.BucketName, _storjService.EncryptionAccess);
                    await _objectService.DeleteObjectAsync(bucket, bucketEntryVM.ObjectInfo.Path);
                }
                catch (Exception ex)
                {
                    Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("Could not delete object - " + ex.Message);
                    await dialog.ShowAsync();
                    return;
                }
            }
            else if (bucketEntryVM.IsUploadOperation)
            {
                ContentDialog cancelObjectUploadDialog = new ContentDialog
                {
                    Title = "Cancel '" + bucketEntryVM.ObjectInfo.Path + "'",
                    Content = "Do you really want to cancel the upload of '" + bucketEntryVM.ObjectInfo.Path + "' ?",
                    CloseButtonText = "No",
                    PrimaryButtonText = "Yes"
                };

                ContentDialogResult result = await cancelObjectUploadDialog.ShowAsync();
                if (result != ContentDialogResult.Primary)
                    return;
                bucketEntryVM.UploadOperation.Cancel();
                try
                {
                    BucketContentViewModel.ActiveUploadOperations[bucketEntryVM._bucketContentViewModel.BucketName].Remove(bucketEntryVM.UploadOperation);
                }
                catch
                {
                    //Ignore any error
                }
            }
            await bucketEntryVM._bucketContentViewModel.RefreshAsync();
        }
    }
}
