using Windows.UI.Xaml.Controls;
using uplink.NET.Sample.Shared.Commands;

namespace uplink.NET.Sample.Shared.Pages
{
    public sealed partial class PlanterPage : Page
    {
        public System.Windows.Input.ICommand GoBackCommand { get; }

        public PlanterPage()
        {
            this.InitializeComponent();
            GoBackCommand = new GoBackCommand();
            this.DataContext = this;
        }
    }
}
