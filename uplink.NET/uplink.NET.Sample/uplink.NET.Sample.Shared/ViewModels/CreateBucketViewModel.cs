using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using uplink.NET.Interfaces;
using uplink.NET.Sample.Shared.Commands;
using uplink.NET.Sample.Shared.Interfaces;

namespace uplink.NET.Sample.Shared.ViewModels
{
    public class CreateBucketViewModel:BaseViewModel
    {
        public string BucketName { get; set; }
        public ICommand SaveBucketCommand { get; set; }
        public ICommand GoBackCommand { get; set; }

        public CreateBucketViewModel(IBucketService bucketService)
        {
            SaveBucketCommand = new SaveBucketCommand(bucketService);
            GoBackCommand = new GoBackCommand();
        }
    }
}
