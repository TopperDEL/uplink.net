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
    public class SaveBucketCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        IBucketService _bucketService;

        public SaveBucketCommand(IBucketService bucketService)
        {
            _bucketService = bucketService;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            CreateBucketViewModel createBucketViewModel = parameter as CreateBucketViewModel;

            try
            {
                await _bucketService.CreateBucketAsync(createBucketViewModel.BucketName);
            }
            catch (Exception ex)
            {
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("Could not create bucket - " + ex.Message);
                await dialog.ShowAsync();
                return;
            }
            var frame = (Windows.UI.Xaml.Controls.Frame)Windows.UI.Xaml.Window.Current.Content;
            frame.Navigate(typeof(BucketListPage));
        }
    }
}
