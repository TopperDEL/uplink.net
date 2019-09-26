using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using uplink.NET.Interfaces;
using uplink.NET.Sample.Shared.Commands;
using uplink.NET.Sample.Shared.Interfaces;
using uplink.NET.Sample.Shared.Services;

namespace uplink.NET.Sample.Shared.ViewModels
{
    public class BucketListViewModel : BaseViewModel
    {
        IBucketService _bucketService;
        public ICommand LogoutCommand { get; private set; }
        public ICommand CreateBucketCommand { get; private set; }
        public ICommand DeleteBucketCommand { get; private set; }

        public ObservableCollection<uplink.NET.Sample.Shared.ViewModels.BucketInfoViewModel> Buckets { get; set; }
        public BucketListViewModel(IBucketService bucketService)
        {
            _bucketService = bucketService;

            LogoutCommand = new LogoutCommand(Factory.LoginService);
            CreateBucketCommand = new CreateBucketCommand();
            DeleteBucketCommand = new DeleteBucketCommand(_bucketService);

            Buckets = new ObservableCollection<NET.Sample.Shared.ViewModels.BucketInfoViewModel>();

            LoadBuckets();
        }

        private async Task LoadBuckets()
        {
            base.StartLoading();

            NET.Models.BucketListOptions listOptions = new NET.Models.BucketListOptions();
            var buckets = await _bucketService.ListBucketsAsync(listOptions);
            foreach (var bucket in buckets.Items)
                Buckets.Add(new BucketInfoViewModel(bucket, _bucketService));

            base.DoneLoading();
        }
    }
}
