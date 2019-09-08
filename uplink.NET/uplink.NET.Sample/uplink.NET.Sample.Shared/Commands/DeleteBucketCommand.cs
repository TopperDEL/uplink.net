using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using uplink.NET.Interfaces;
using uplink.NET.Sample.Shared.Interfaces;
using uplink.NET.Sample.Shared.Pages;
using uplink.NET.Sample.Shared.Services;
using uplink.NET.Sample.Shared.ViewModels;

namespace uplink.NET.Sample.Shared.Commands
{
    public class DeleteBucketCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        IBucketService _bucketService;
        IStorjService _storjService;

        public DeleteBucketCommand(IBucketService bucketService, IStorjService storjService)
        {
            _bucketService = bucketService;
            _storjService = storjService;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            //ToDo: Ask for deletion
            BucketInfoViewModel bucketInfoVM = parameter as BucketInfoViewModel;
            try
            {
                await _bucketService.DeleteBucketAsync(_storjService.Project, bucketInfoVM.BucketInfo.Name);
            }
            catch(Exception ex)
            {
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("Could not delete bucket - " + ex.Message);
                await dialog.ShowAsync();
                return;
            }
            var frame = (Windows.UI.Xaml.Controls.Frame)Windows.UI.Xaml.Window.Current.Content;
            frame.Navigate(typeof(BucketListPage));
        }
    }
}
