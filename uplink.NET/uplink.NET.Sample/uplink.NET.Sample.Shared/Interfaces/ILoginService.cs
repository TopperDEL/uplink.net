using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Sample.Shared.Models;

namespace uplink.NET.Sample.Shared.Interfaces
{
    public interface ILoginService
    {
        Task<bool> LoginAsync(LoginData loginData);
        bool IsLoggedIn();
        LoginData GetLoginData();
        bool Logout();
    }
}
