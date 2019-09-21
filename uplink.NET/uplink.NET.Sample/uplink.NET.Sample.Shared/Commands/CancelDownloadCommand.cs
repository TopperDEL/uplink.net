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
    public class CancelDownloadCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        IBucketService _bucketService;
        IObjectService _objectService;
        IStorjService _storjService;

        public CancelDownloadCommand(IBucketService bucketService, IObjectService objectService, IStorjService storjService)
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

            ContentDialog cancelObjectDownloadDialog = new ContentDialog
            {
                Title = "Cancel '" + bucketEntryVM.DownloadOperation.ObjectName + "'",
                Content = "Do you really want to cancel the download of '" + bucketEntryVM.DownloadOperation.ObjectName + "' ?",
                CloseButtonText = "No",
                PrimaryButtonText = "Yes"
            };

            ContentDialogResult result = await cancelObjectDownloadDialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
                return;
            bucketEntryVM.DownloadOperation.Cancel();
            try
            {
                BucketContentViewModel.ActiveDownloadOperations[bucketEntryVM._bucketContentViewModel.BucketName].Remove(bucketEntryVM.DownloadOperation);
            }
            catch
            {
                //Ignore any error
            }
        }
    }
}
