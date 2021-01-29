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
    public class CancelUploadCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        IBucketService _bucketService;
        IObjectService _objectService;

        public CancelUploadCommand(IBucketService bucketService, IObjectService objectService)
        {
            _bucketService = bucketService;
            _objectService = objectService;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            BucketEntryViewModel bucketEntryVM = parameter as BucketEntryViewModel;

            ContentDialog cancelObjectUploadDialog = new ContentDialog
            {
                Title = "Cancel '" + bucketEntryVM.UploadOperation.ObjectName + "'",
                Content = "Do you really want to cancel the upload of '" + bucketEntryVM.UploadOperation.ObjectName + "' ?",
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

            await bucketEntryVM._bucketContentViewModel.RefreshAsync();
        }
    }
}
