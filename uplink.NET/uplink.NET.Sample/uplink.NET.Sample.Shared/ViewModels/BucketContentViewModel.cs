using System;
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
        public ObservableCollection<BucketEntryViewModel> Entries { get; set; }
        public string BucketName { get; private set; }
        public ICommand GoBackCommand { get; set; }
        public ICommand UploadFileCommand { get; set; }

        IObjectService _objectService;
        IBucketService _bucketService;
        IStorjService _storjService;
        ILoginService _loginService;

        public BucketContentViewModel(IObjectService objectService, IBucketService bucketService, IStorjService storjService, ILoginService loginService)
        {
            Entries = new ObservableCollection<BucketEntryViewModel>();

            _objectService = objectService;
            _bucketService = bucketService;
            _storjService = storjService;
            _loginService = loginService;

            GoBackCommand = new GoBackCommand();
        }

        public void SetBucketName(string bucketName)
        {
            BucketName = bucketName;
            UploadFileCommand = new UploadFileCommand(this, _objectService, _bucketService, _storjService, _loginService, BucketName);
        }

        public void AddUploadOperation(UploadOperation uploadOperation)
        {
            var entry = new BucketEntryViewModel(this);
            entry.IsUploadOperation = true;
            entry.UploadOperation = uploadOperation;
            entry.InitUploadOperation();
            Entries.Add(entry);
        }

        public async Task Refresh()
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

            //Load all options
            try
            {
                var bucket = await _bucketService.OpenBucketAsync(_storjService.Project, BucketName, _storjService.EncryptionAccess);
                var listOptions = new ListOptions();
                listOptions.Direction = ListDirection.STORJ_AFTER;
                var objects = await _objectService.ListObjectsAsync(bucket, listOptions);
                foreach (var obj in objects.Items)
                {
                    var entry = new BucketEntryViewModel(this);
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

            DoneLoading();
        }
    }
}
