using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Interfaces;
using uplink.NET.Sample.Shared.Interfaces;
using uplink.NET.Services;

namespace uplink.NET.Sample.Shared.Services
{
    public static class Factory
    {
        public static IStorjEnvironment StorjEnvironment { get; set; }

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
                    _bucketService = new BucketService(StorjEnvironment);
                return _bucketService;
            }
        }

        private static IObjectService _objectService;
        public static IObjectService ObjectService
        {
            get
            {
                if (_objectService == null)
                    _objectService = new ObjectService();
                return _objectService;
            }
        }
    }
}
