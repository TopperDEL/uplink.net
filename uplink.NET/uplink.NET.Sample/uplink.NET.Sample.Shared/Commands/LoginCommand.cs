using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using uplink.NET.Sample.Shared.Interfaces;
using uplink.NET.Sample.Shared.Pages;
using uplink.NET.Sample.Shared.ViewModels;

namespace uplink.NET.Sample.Shared.Commands
{
    public class LoginCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private ILoginService _loginService;

        public LoginCommand(ILoginService loginService)
        {
            _loginService = loginService;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            LoginViewModel viewModel = parameter as LoginViewModel;

            var loggedIn = await _loginService.LoginAsync(viewModel.LoginData);

            if (loggedIn)
            {
                var frame = (Windows.UI.Xaml.Controls.Frame)Windows.UI.Xaml.Window.Current.Content;
                frame.Navigate(typeof(MainPage));
            }
        }
    }
}
