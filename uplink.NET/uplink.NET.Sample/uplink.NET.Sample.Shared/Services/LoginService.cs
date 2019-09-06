using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Sample.Shared.Interfaces;
using uplink.NET.Sample.Shared.Models;
using Windows.Storage;

namespace uplink.NET.Sample.Shared.Services
{
    public class LoginService : ILoginService
    {
        const string APIKEY = "APIKey";
        const string SECRET = "Secret";

        ApplicationDataContainer _localSettings;
        LoginData _loginData;

        public LoginService()
        {
            _localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            _loginData = new LoginData();
            if (_localSettings.Values.ContainsKey(APIKEY))
                _loginData.APIKey = (string)_localSettings.Values[APIKEY];
            if (_localSettings.Values.ContainsKey(SECRET))
                _loginData.Secret = (string)_localSettings.Values[SECRET];
        }

        public LoginData GetLoginData()
        {
            return _loginData;
        }

        public bool IsLoggedIn()
        {
            if (!string.IsNullOrEmpty(_loginData.APIKey) && !string.IsNullOrEmpty(_loginData.Secret))
                return true;
            else
                return false;
        }

        public bool Login(LoginData loginData)
        {
            _loginData = loginData;
            _localSettings.Values[APIKEY] = _loginData.APIKey;
            _localSettings.Values[SECRET] = _loginData.Secret;

            return true;
        }

        public bool Logout()
        {
            _localSettings.Values.Remove(APIKEY);
            _localSettings.Values.Remove(SECRET);
            _loginData = new LoginData();

            return true;
        }
    }
}
