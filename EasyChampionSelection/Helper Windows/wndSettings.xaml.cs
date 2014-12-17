using EasyChampionSelection.ECS;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace EasyChampionSelection.Helper_Windows {
    /// <summary>
    /// Interaction logic for wndSettings.xaml
    /// </summary>
    public partial class wndSettings : Window {
        private Settings _s;

        public wndSettings(Settings s) {
            if(s == null) {
                throw new ArgumentNullException();
            }

            InitializeComponent();
            _s = s;

            txtApiKey.Text = s.UserApiKey;
            chkShowMainFormOnBoot.IsChecked = s.ShowMainFormOnLaunch;

            lblChampionSearchBar.Content = "Champion Searchbar: " + s.ChampionSearchbarRelativePos.ToString();
            lblClientOverlay.Content = "Client Overlay: " + s.ClientOverlayRelativePos.ToString();
            lblTeamChat.Content = "Team Chat: " + s.TeamChatRelativePos.ToString();
            this.SizeToContent = System.Windows.SizeToContent.Width;
        }

        private void txtApiKey_TextChanged(object sender, TextChangedEventArgs e) {
            _s.UserApiKey = txtApiKey.Text;
        }

        private void chkShowMainFormOnBoot_Checked(object sender, RoutedEventArgs e) {
            _s.ShowMainFormOnLaunch = true;
        }

        private void chkShowMainFormOnBoot_Unchecked(object sender, RoutedEventArgs e) {
            _s.ShowMainFormOnLaunch = false;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
