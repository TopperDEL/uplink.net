using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using uplink.NET.Interfaces;
using uplink.NET.Sample.Shared.Interfaces;
using uplink.NET.Sample.Shared.Pages;
using uplink.NET.Sample.Shared.Services;
using uplink.NET.Sample.Shared.ViewModels;

namespace uplink.NET.Sample.Shared.Commands
{
    public class GoBackCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public GoBackCommand()
        {
        }

        public bool CanExecute(object parameter)
        {
            var frame = (Windows.UI.Xaml.Controls.Frame)Windows.UI.Xaml.Window.Current.Content;
            return frame.CanGoBack;
        }

        public void Execute(object parameter)
        {
            var frame = (Windows.UI.Xaml.Controls.Frame)Windows.UI.Xaml.Window.Current.Content;
            frame.GoBack();
        }
    }
}
