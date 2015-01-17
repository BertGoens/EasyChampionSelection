using EasyChampionSelection.ECS;
using System.Diagnostics;
using System.Windows;

namespace EasyChampionSelection.Helper_Windows {
    /// <summary>
    /// Interaction logic for wndCredits.xaml
    /// </summary>
    public partial class wndCredits : Window {
        public wndCredits() {
            InitializeComponent();
            StaticWindowUtilities.EnsureVisibility(this);
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
