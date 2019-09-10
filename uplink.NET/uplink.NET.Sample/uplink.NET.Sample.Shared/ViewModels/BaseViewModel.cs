using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace uplink.NET.Sample.Shared.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool Loading { get; set; }
        public bool Loaded { get; set; }
        public bool IsBusy { get; set; }

        public BaseViewModel()
        {
            Loading = true;
            Loaded = false;
        }

        public void DoneLoading()
        {
            Loading = false;
            Loaded = true;
            RaiseChanged(nameof(Loading));
            RaiseChanged(nameof(Loaded));
        }

        public void StartLoading()
        {
            Loading = true;
            Loaded = false;
            RaiseChanged(nameof(Loading));
            RaiseChanged(nameof(Loaded));
        }

        protected async void RaiseChanged(string propertyName)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,()=>
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
                }
            });
        }
    }
}
