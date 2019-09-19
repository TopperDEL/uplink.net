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

        BucketContentViewModel _senderView;
        IObjectService _objectService;
        IBucketService _bucketService;
        IStorjService _storjService;
        ILoginService _loginService;
        string _bucketName;

        public UploadFileCommand(BucketContentViewModel senderView, IObjectService objectService, IBucketService bucketService, IStorjService storjService, ILoginService loginService, string bucketName)
        {
            _senderView = senderView;
            _objectService = objectService;
            _bucketService = bucketService;
            _storjService = storjService;
            _loginService = loginService;
            _bucketName = bucketName;
        }

        public bool CanExecute(object parameter)
        {
            bool selectVideo = Convert.ToBoolean(parameter);

            if (selectVideo)
                return CrossMedia.Current.IsPickVideoSupported;
            else
                return CrossMedia.Current.IsPickPhotoSupported;
        }

        public async void Execute(object parameter)
        {
            bool selectVideo = Convert.ToBoolean(parameter);

            Plugin.Media.Abstractions.MediaFile galleryObject;

            if (selectVideo)
                galleryObject = await CrossMedia.Current.PickVideoAsync();
            else
                galleryObject = await CrossMedia.Current.PickPhotoAsync();

            if (galleryObject == null)
                return;

            Uri file = new Uri(galleryObject.AlbumPath);
            var filename = System.IO.Path.GetFileName(file.LocalPath);

            var stream = galleryObject.GetStream();

            var bucket = await _bucketService.OpenBucketAsync(_storjService.Project, _bucketName, _storjService.EncryptionAccess);
            var uploadOptions = new UploadOptions();
            uploadOptions.Expires = DateTime.MaxValue;
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            var uploadOperation = await _objectService.UploadObjectAsync(bucket, filename, uploadOptions, bytes, true);
            if (BucketContentViewModel.ActiveUploadOperations.ContainsKey(_bucketName))
                BucketContentViewModel.ActiveUploadOperations[_bucketName].Add(uploadOperation);
            else
            {
                var list = new List<UploadOperation>();
                list.Add(uploadOperation);
                BucketContentViewModel.ActiveUploadOperations.Add(_bucketName, list);
            }
            _senderView.AddUploadOperation(uploadOperation);
        }
    }
}
