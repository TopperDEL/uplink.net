using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using uplink.NET.Models;
using uplink.NET.Sample.Shared.Interfaces;
using uplink.NET.Sample.Shared.Pages;
using uplink.NET.Sample.Shared.Services;
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
                try
                {
                    Factory.Scope = new Scope(viewModel.LoginData.APIKey, viewModel.LoginData.Satellite, viewModel.LoginData.Secret);

                    Windows.UI.Popups.MessageDialog attentionDialog = new Windows.UI.Popups.MessageDialog("This app is only for testing - it might contain errors and corrupt your data. Use at your own risk!", "Attention");
                    await attentionDialog.ShowAsync();

                    var frame = (Windows.UI.Xaml.Controls.Frame)Windows.UI.Xaml.Window.Current.Content;
                    frame.Navigate(typeof(BucketListPage));
                }
                catch (Exception ex)
                {
                    Windows.UI.Popups.MessageDialog errorDialog = new Windows.UI.Popups.MessageDialog("Could not connect to storj: " + ex.Message);
                    await errorDialog.ShowAsync();
                }
            }
        }
    }
}
