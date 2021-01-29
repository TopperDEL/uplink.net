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
    public class ShowErrorCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public ShowErrorCommand()
        {
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            BucketEntryViewModel bucketEntryVM = parameter as BucketEntryViewModel;

            if (bucketEntryVM.IsDownloadOperation)
            {
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog(bucketEntryVM.DownloadOperation.ErrorMessage);
                await dialog.ShowAsync();
            }
            else if (bucketEntryVM.IsUploadOperation)
            {
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog(bucketEntryVM.UploadOperation.ErrorMessage);
                await dialog.ShowAsync();
            }
        }
    }
}
