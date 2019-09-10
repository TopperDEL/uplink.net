using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using uplink.NET.Sample.Shared.Services;
using uplink.NET.Sample.Shared.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace uplink.NET.Sample.Shared.Pages
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class BucketContentPage : Page
    {
        public BucketContentViewModel _vm;
        public BucketContentPage()
        {
            this.InitializeComponent();
            this.DataContext = _vm = new BucketContentViewModel(Factory.ObjectService, Factory.BucketService, Factory.StorjService, Factory.LoginService);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            BucketInfoViewModel bucketVM = e.Parameter as BucketInfoViewModel;
            _vm.SetBucketName(bucketVM.BucketInfo.Name);
            _vm.InitAsync();
        }

        private void UploadFile_Click(object sender, RoutedEventArgs e)
        {
#if __ANDROID__
            _vm.UploadFileCommand.Execute(null);
#endif
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
#if __ANDROID__
            _vm.GoBackCommand.Execute(null);
#endif
        }
    }
}
