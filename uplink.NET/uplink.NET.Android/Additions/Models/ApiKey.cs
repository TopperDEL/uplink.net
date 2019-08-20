using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using uplink.NET.Contracts.Models;

namespace uplink.NET.Android.Binding.Additions.Models
{
    public class ApiKey : IApiKey
    {
        private string _apiKey;

        public ApiKey(string apiKey)
        {
            _apiKey = apiKey;
        }

        public void Dispose()
        {
        }

        public string GetApiKey()
        {
            return _apiKey;
        }
    }
}