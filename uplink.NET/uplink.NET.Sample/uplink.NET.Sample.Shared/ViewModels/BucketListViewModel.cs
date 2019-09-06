using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Interfaces;
using uplink.NET.Sample.Shared.Interfaces;

namespace uplink.NET.Sample.Shared.ViewModels
{
    public class BucketListViewModel : BaseViewModel
    {
        IBucketService _bucketService;
        IStorjService _storjService;
        public ObservableCollection<uplink.NET.Models.BucketInfo> Buckets { get; set; }
        public BucketListViewModel(IBucketService bucketService, IStorjService storjService)
        {
            _bucketService = bucketService;
            _storjService = storjService;

            Buckets = new ObservableCollection<NET.Models.BucketInfo>();

            LoadBuckets();
        }

        private async Task LoadBuckets()
        {
            base.StartLoading();

            NET.Models.BucketListOptions listOptions = new NET.Models.BucketListOptions();
            var buckets = await _bucketService.ListBucketsAsync(_storjService.Project, listOptions);
            foreach (var bucket in buckets.Items)
                Buckets.Add(bucket);

            base.DoneLoading();
        }
    }
}
