using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Sample.Shared.Commands;
using uplink.NET.Sample.Shared.Interfaces;
using uplink.NET.Sample.Shared.Models;

namespace uplink.NET.Sample.Shared.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public LoginData LoginData { get; set; }

        public LoginCommand LoginCommand { get; set; }

        public string StorjVersion
        {
            get
            {
                return uplink.NET.Models.Access.GetStorjVersion();
            }
        }

        public string UplinkVersion
        {
            get
            {
                return System.Reflection.Assembly.GetAssembly(typeof(uplink.NET.Models.Access)).GetName().Version.ToString();
            }
        }

        public LoginViewModel(ILoginService loginService)
        {
            LoginData = new LoginData();

            LoginCommand = new LoginCommand(loginService);
        }
    }
}
