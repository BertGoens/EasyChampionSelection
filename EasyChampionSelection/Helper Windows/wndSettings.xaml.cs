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
        private StaticPinvokeLolClient _lcg;
        private wndConfigLolClientOverlay _wndCLCO;

        public wndSettings(Settings s, StaticPinvokeLolClient lcg, bool openConfigLolClientOverlay = false) {
            if(s == null) {
                throw new ArgumentNullException();
            }

            InitializeComponent();
            _s = s;
            _lcg = lcg;

            _s.ChampionSearchbarChanged += _s_ChampionSearchbarChanged;
            _s.ClientOverlayChanged += _s_ClientOverlayChanged;
            _s.TeamChatChanged += _s_TeamChatChanged;

            _wndCLCO = new wndConfigLolClientOverlay(_lcg, _s);

            txtApiKey.Text = s.UserApiKey;
            chkShowMainFormOnBoot.IsChecked = s.ShowMainFormOnLaunch;

            lblChampionSearchBar.Content += " " +_s.ChampionSearchbarRelativePos.ToString();
            lblClientOverlay.Content += " " + _s.ClientOverlayRelativePos.ToString();
            lblTeamChat.Content += " " +_s.TeamChatRelativePos.ToString();

            lblApplicationpath.Content += " " + StaticSerializer.applicationPath();
            lblAppDataPath.Content += " " + StaticSerializer.userAppDataPath();

            if(_lcg == null && !File.Exists(StaticSerializer.FullPath_ClientImage)) {
                btnConfigClientOverlay.IsEnabled = false;
            }

            if(openConfigLolClientOverlay) {
                OpenConfigClientOverlay(null, null);
            }
        }

       private void _s_TeamChatChanged(Settings sender, EventArgs e) {
            lblTeamChat.Content = "Team Chatbar: " + _s.TeamChatRelativePos.ToString();
        }

        private void _s_ClientOverlayChanged(Settings sender, EventArgs e) {
            lblClientOverlay.Content = "ECS Overlay: " + _s.ClientOverlayRelativePos.ToString();
        }

        private void _s_ChampionSearchbarChanged(Settings sender, EventArgs e) {
            lblChampionSearchBar.Content = "Champion Searchbar: " + _s.ChampionSearchbarRelativePos.ToString();
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

        private void lblApplicationpath_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Process.Start(StaticSerializer.applicationPath());
        }

        private void lblApplicationAppData_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Process.Start(StaticSerializer.userAppDataPath());
        }        
    }
}
