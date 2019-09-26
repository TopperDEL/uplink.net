using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using uplink.NET.Interfaces;
using uplink.NET.Models;
using uplink.NET.Sample.Shared.Commands;
using uplink.NET.Sample.Shared.Interfaces;

namespace uplink.NET.Sample.Shared.ViewModels
{
    public class BucketInfoViewModel: BaseViewModel
    {
        public BucketInfo BucketInfo { get; private set; }
        public ICommand DeleteBucketCommand { get; private set; }
        public ICommand OpenBucketCommand { get; private set; }

        public BucketInfoViewModel(BucketInfo bucketInfo, IBucketService bucketService)
        {
            BucketInfo = bucketInfo;
            DeleteBucketCommand = new DeleteBucketCommand(bucketService); ;
            OpenBucketCommand = new OpenBucketCommand(); ;
        }
    }
}
