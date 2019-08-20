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

namespace uplink.NET.Android.Binding.Additions.Models
{
    public class Project : uplink.NET.Contracts.Models.IProject
    {
        internal IO.Storj.Libuplink.Mobile.Project _projectRef = null;

        public Project(Uplink uplink, ApiKey apiKey, string satelliteAddr, ProjectOptions projectOptions)
        {
            _projectRef = uplink._uplinkRef.OpenProject(satelliteAddr, apiKey.GetApiKey());
        }

        public void Dispose()
        {
            if(_projectRef != null)
            {
                _projectRef.Dispose();
                _projectRef = null;
            }
        }
    }
}