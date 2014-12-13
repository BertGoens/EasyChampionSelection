using EasyChampionSelection.ECS;
using System;
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
            chkStartOnBoot.IsChecked = s.StartOnBoot;
            chkShowMainFormOnBoot.IsChecked = s.ShowMainFormOnLaunch;

            lblChampionSearchBar.Content = "Champion Searchbar: " + s.ChampionSearchbarRelativePos.ToString();
            lblClientOverlay.Content = "Client Overlay: " + s.ClientOverlayRelativePos.ToString();
            lblTeamChat.Content = "Team Chat: " + s.TeamChatRelativePos.ToString();
        }

        private void txtApiKey_TextChanged(object sender, TextChangedEventArgs e) {
            _s.UserApiKey = txtApiKey.Text;
        }

        private void chkStartOnBoot_Checked(object sender, RoutedEventArgs e) {
            _s.StartOnBoot = true;
        }

        private void chkStartOnBoot_Unchecked(object sender, RoutedEventArgs e) {
            _s.StartOnBoot = false;
        }

        private void chkShowMainFormOnBoot_Checked(object sender, RoutedEventArgs e) {
            _s.ShowMainFormOnLaunch = true;
        }

        private void chkShowMainFormOnBoot_Unchecked(object sender, RoutedEventArgs e) {
            _s.ShowMainFormOnLaunch = false;
        }
    }
}
