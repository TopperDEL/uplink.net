using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Sample.Shared.Models;

namespace uplink.NET.Sample.Shared.Interfaces
{
    public interface ILoginService
    {
        bool Login(LoginData loginData);
        bool IsLoggedIn();
        LoginData GetLoginData();
        bool Logout();
    }
}
