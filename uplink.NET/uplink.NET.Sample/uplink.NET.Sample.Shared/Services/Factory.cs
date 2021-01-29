using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Interfaces;
using uplink.NET.Models;
using uplink.NET.Sample.Shared.Interfaces;
using uplink.NET.Services;

namespace uplink.NET.Sample.Shared.Services
{
    public static class Factory
    {
        public static Access Access { get; set; }

        private static ILoginService _loginService;
        public static ILoginService LoginService
        {
            get
            {
                if (_loginService == null)
                    _loginService = new LoginService();
                return _loginService;
            }
        }

        private static IBucketService _bucketService;
        public static IBucketService BucketService
        {
            get
            {
                if (_bucketService == null)
                    _bucketService = new BucketService(Access);
                return _bucketService;
            }
        }

        private static IObjectService _objectService;
        public static IObjectService ObjectService
        {
            get
            {
                if (_objectService == null)
                    _objectService = new ObjectService(Access);
                return _objectService;
            }
        }
    }
}
