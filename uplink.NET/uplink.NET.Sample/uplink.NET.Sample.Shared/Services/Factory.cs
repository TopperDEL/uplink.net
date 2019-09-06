﻿using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Interfaces;
using uplink.NET.Sample.Shared.Interfaces;
using uplink.NET.Services;

namespace uplink.NET.Sample.Shared.Services
{
    public static class Factory
    {
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
                    _bucketService = new BucketService();
                return _bucketService;
            }
        }

        private static IStorjService _storjService;
        public static IStorjService StorjService
        {
            get
            {
                if (_storjService == null)
                    _storjService = new StorjService();
                return _storjService;
            }
        }
    }
}
