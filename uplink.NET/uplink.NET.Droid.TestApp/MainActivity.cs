using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace uplink.NET.Droid.TestApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View) sender;

            uplink.NET.Services.BucketService service = new Services.BucketService();
            uplink.NET.Models.UplinkConfig uplinkconfig = new Models.UplinkConfig();
            uplink.NET.Models.Uplink uplink = new Models.Uplink(uplinkconfig);
            uplink.NET.Models.ApiKey apikey = new Models.ApiKey("13Yqe1K83MdaH7WJkR9qmsXaetDgQFzEiHmxCd3QMQfkZe1Hr2mYE8sbaoG6f74tYGQ7QmMt2bdaCJJy8dXLgJNyKKfzkeWkLa4ib9H");
            uplink.NET.Models.ProjectOptions projectOptions = new Models.ProjectOptions();
            uplink.NET.Models.Project project = new Models.Project(uplink, apikey, "europe-west-1.tardigrade.io:7777", projectOptions);
            uplink.NET.Models.BucketConfig bucketConfig = new Models.BucketConfig();
            var result = service.CreateBucket(project, "AndroidBucket", bucketConfig);

            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}

