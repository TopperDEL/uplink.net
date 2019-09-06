using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Sample.Shared.Interfaces;

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
    }
}
