using EasyChampionSelection.ECS;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace EasyChampionSelection.Helper_Windows {
    /// <summary>
    /// Interaction logic for wndSettings.xaml
    /// </summary>
    public partial class wndSettings : Window {
        private Settings _s;
        private StaticLolClientGraphics _lcg;
        private wndConfigLolClientOverlay _wndCLCO;

        public wndSettings(Settings s, StaticLolClientGraphics lcg, bool openConfigLolClientOverlay = false) {
            if(s == null) {
                throw new ArgumentNullException();
            }

            InitializeComponent();
            _s = s;
            _lcg = lcg;
            _wndCLCO = new wndConfigLolClientOverlay(_lcg, _s);

            txtApiKey.Text = s.UserApiKey;
            chkShowMainFormOnBoot.IsChecked = s.ShowMainFormOnLaunch;

            lblChampionSearchBar.Content = "Champion Searchbar: " + s.ChampionSearchbarRelativePos.ToString();
            lblClientOverlay.Content = "Client Overlay: " + s.ClientOverlayRelativePos.ToString();
            lblTeamChat.Content = "Team Chat: " + s.TeamChatRelativePos.ToString();
            this.SizeToContent = System.Windows.SizeToContent.Width;

            if(_lcg == null && !File.Exists(StaticSerializer.FullPath_ClientImage)) {
                btnConfigClientOverlay.IsEnabled = false;
            }

            if(openConfigLolClientOverlay) {
                OpenConfigClientOverlay(null, null);
            }
        }

        /// <summary>
        /// Is child window wndConfigLolClientOverlay open?
        /// </summary>
        public bool IsConfigLolClientOverlayOpened() {
            return _wndCLCO.IsLoaded;
        }

        /// <summary>
        /// Closes wndConfigLolClientOverlay and itself
        /// </summary>
        public void SafeClose() {
            if(_wndCLCO.IsLoaded) {
                _wndCLCO.Close();
            }
            this.Close();
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

        private void OpenConfigClientOverlay(object sender, RoutedEventArgs e) {
            if(_wndCLCO.IsLoaded == false) {
                _wndCLCO = new wndConfigLolClientOverlay(_lcg, _s);
            }

            _wndCLCO.Show();
        }
    }
}
