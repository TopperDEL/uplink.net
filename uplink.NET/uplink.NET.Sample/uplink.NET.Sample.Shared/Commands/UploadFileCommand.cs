using Plugin.Media;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using uplink.NET.Interfaces;
using uplink.NET.Models;
using uplink.NET.Sample.Shared.Interfaces;
using uplink.NET.Sample.Shared.Pages;
using uplink.NET.Sample.Shared.Services;
using uplink.NET.Sample.Shared.ViewModels;

namespace uplink.NET.Sample.Shared.Commands
{
    public class UploadFileCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        IObjectService _objectService;
        IBucketService _bucketService;
        IStorjService _storjService;
        ILoginService _loginService;
        string _bucketName;

        public UploadFileCommand(IObjectService objectService, IBucketService bucketService, IStorjService storjService, ILoginService loginService, string bucketName)
        {
            _objectService = objectService;
            _bucketService = bucketService;
            _storjService = storjService;
            _loginService = loginService;
            _bucketName = bucketName;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            var photo = await CrossMedia.Current.PickPhotoAsync();
            var stream = photo.GetStream();

            var bucket = await _bucketService.OpenBucketAsync(_storjService.Project, _bucketName, EncryptionAccess.FromPassphrase(_storjService.Project, _loginService.GetLoginData().Secret));
            var uploadOptions = new UploadOptions();
            uploadOptions.Expires = DateTime.MaxValue;
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            var uploadOperation = await _objectService.UploadObjectAsync(bucket, Guid.NewGuid().ToString() + ".jpg", uploadOptions, bytes, true);
            if (BucketContentViewModel.ActiveUploadOperations.ContainsKey(_bucketName))
                BucketContentViewModel.ActiveUploadOperations[_bucketName].Add(uploadOperation);
            else
            {
                var list = new List<UploadOperation>();
                list.Add(uploadOperation);
                BucketContentViewModel.ActiveUploadOperations.Add(_bucketName, list);
            }
            //var frame = (Windows.UI.Xaml.Controls.Frame)Windows.UI.Xaml.Window.Current.Content;
            //frame.Navigate(typeof(CreateBucketPage));
        }
    }
}
