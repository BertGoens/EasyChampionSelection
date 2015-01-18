using EasyChampionSelection.ECS;
using EasyChampionSelection.ECS.AppRuntimeResources;
using EasyChampionSelection.ECS.AppRuntimeResources.LolClient;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace EasyChampionSelection.Helper_Windows {
    /// <summary>
    /// Interaction logic for wndSettings.xaml
    /// </summary>
    public partial class wndSettings : Window {
        private EcsSettings _s;
        private StaticPinvokeLolClient _pilc;
        private wndConfigLolClientOverlay _wndCLCO;
        private Action<string> _displayMessage;

        private wndSettings() {
            InitializeComponent();
            StaticWindowUtilities.EnsureVisibility(this);
        }

        public wndSettings(EcsSettings s, StaticPinvokeLolClient pilc, Action<string> DisplayMessage)
            : this() {
            if(s == null || pilc == null || DisplayMessage == null) {
                throw new ArgumentNullException();
            }

            _s = s;
            _displayMessage = DisplayMessage;
            _pilc = pilc;

            _s.ChampionSearchbarChanged += _s_ChampionSearchbarChanged;
            _s.ClientOverlayChanged += _s_ClientOverlayChanged;
            _s.TeamChatChanged += _s_TeamChatChanged;

            double dotNetVersion = DotnetRegistryVersion();
            if(dotNetVersion < 4.5) {
                gbBasicRequirements.Visibility = System.Windows.Visibility.Visible;
                spDotNetVersion.Visibility = System.Windows.Visibility.Visible;
                lblDotNetVersion.Content += " " + dotNetVersion;
            }

            if(!isRunFromProgramFiles()) {
                gbBasicRequirements.Visibility = System.Windows.Visibility.Visible;
                lblProgramFiles.Visibility = Visibility.Visible;
                lblProgramFiles.Content = "Not run from " + Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            }

            txtApiKey.Text = s.UserApiKey;
            chkShowMainFormOnLaunch.IsChecked = s.ShowMainFormOnLaunch;
            chkStartLeagueWithECS.IsChecked = s.StartLeagueWithEcs;
            txtLeaguePath.Text = s.LeaguePath;

            lblChampionSearchBar.Content += " " + _s.ChampionSearchbarRelativePos.ToString();
            lblClientOverlay.Content += " " + _s.ClientOverlayRelativePos.ToString();
            lblTeamChat.Content += " " + _s.TeamChatRelativePos.ToString();

            lblApplicationpath.Content += " " + StaticSerializer.applicationPath();
            lblAppDataPath.Content += " " + StaticSerializer.userAppDataPath();

            if(pilc.ClientState == LolClientState.NoClient && !File.Exists(StaticSerializer.FullPath_ClientImage)) {
                btnConfigClientOverlay.IsEnabled = false;
            }

            pilc.LolClientStateChanged += pilc_LolClientStateChanged;

            DispatcherTimer dptm = new DispatcherTimer(DispatcherPriority.Loaded);
            dptm.Interval = new TimeSpan(0, 0, 5);
            dptm.Tick += dptm_Tick;
            dptm.Start();
        }

        void pilc_LolClientStateChanged(StaticPinvokeLolClient sender, PinvokeLolClientEventArgs e) {
            if(e.CurrentState == LolClientState.NoClient) {
                if(!File.Exists(StaticSerializer.FullPath_ClientImage)) {
                    btnConfigClientOverlay.IsEnabled = false;
                }
            }
        }

        void dptm_Tick(object sender, EventArgs e) {
            DispatcherTimer dptm = (DispatcherTimer)sender;
            dptm.Stop();
            showDownloadNewVersion();
        }

        private async void showDownloadNewVersion() {
            if(_s.Version < await _s.OnlineVersion()) {
                spVersion.Visibility = System.Windows.Visibility.Visible;
                lblVersion.Content = "Your version: " + _s.Version + ", latest version: " + await _s.OnlineVersion();
            }
        }

        private void _s_TeamChatChanged(EcsSettings sender, EventArgs e) {
            lblTeamChat.Content = "Team Chatbar: " + _s.TeamChatRelativePos.ToString();
        }

        private void _s_ClientOverlayChanged(EcsSettings sender, EventArgs e) {
            lblClientOverlay.Content = "ECS Overlay: " + _s.ClientOverlayRelativePos.ToString();
        }

        private void _s_ChampionSearchbarChanged(EcsSettings sender, EventArgs e) {
            lblChampionSearchBar.Content = "Champion Searchbar: " + _s.ChampionSearchbarRelativePos.ToString();
        }

        private void btnConfigClientOverlay_Click(object sender, RoutedEventArgs e) {
            if(_wndCLCO == null) {
                _wndCLCO = new wndConfigLolClientOverlay(_pilc, _s, _displayMessage);
                _wndCLCO.Closed += (s, args) => _wndCLCO = null;
            } else {
                StaticWindowUtilities.EnsureVisibility(_wndCLCO);
            }
            _wndCLCO.Show();
        }

        private void txtApiKey_TextChanged(object sender, TextChangedEventArgs e) {
            if(_s.UserApiKey != txtApiKey.Text) {
                _s.UserApiKey = txtApiKey.Text;
                _displayMessage("Loading champions with API");
            }
        }

        private void chkShowMainFormOnBoot_CheckChanged(object sender, RoutedEventArgs e) {
            if(sender == null) {
                return;
            }
            CheckBox tSender = (CheckBox)sender;
            _s.ShowMainFormOnLaunch = (bool)tSender.IsChecked;
        }

        private void chkStartLeagueWithECS_CheckChanged(object sender, RoutedEventArgs e) {
            if(sender == null) {
                return;
            }
            CheckBox tSender = (CheckBox)sender;
            _s.StartLeagueWithEcs = (bool)tSender.IsChecked;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void lblApplicationpath_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Process.Start(StaticSerializer.applicationPath());
        }

        private void lblApplicationAppData_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Process.Start(StaticSerializer.userAppDataPath());
        }

        private void txtLeaguePath_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Select the league of legends launcher";
            string leageDir = _s.LeaguePath;
            if(leageDir.Length > 0) {
                leageDir = leageDir.Substring(0, leageDir.LastIndexOf("\\"));
                if(Directory.Exists(leageDir)) {
                    dlg.InitialDirectory = leageDir;
                }
            }
            // Set filter for file extension and default file extension
            dlg.DefaultExt = "lol.launcher.exe";
            dlg.Filter = "lol.launcher.exe|lol.launcher.exe";
            dlg.FileOk += dlg_FileOk;
            // Display OpenFileDialog by calling ShowDialog method
            if(dlg.ShowDialog(this) == true) {
                _s.LeaguePath = dlg.FileName;
            }
        }

        void dlg_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
            OpenFileDialog dlg = (OpenFileDialog)sender;
            _s.LeaguePath = dlg.FileName;
            txtLeaguePath.Text = dlg.FileName;
        }

        private void btnGoogleLatestDotNetVersion_Click(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo(@"https://www.google.be/search?q=latest+.net+4.5+version"));
        }

        private void lblProgramFiles_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Process.Start(new ProcessStartInfo(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)));
        }

        private void btnDownloadPageNewVersion_Click(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo(@"https://github.com/BertGoens/EasyChampionSelection"));
        }

        private bool isRunFromProgramFiles() {
            string path = AppDomain.CurrentDomain.BaseDirectory.ToString();
            string programfileX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            if(path.IndexOf(programfileX86, StringComparison.OrdinalIgnoreCase) >= 0) {
                return true;
            }
            return false;
        }

        private double versionToDouble(string version) {
            if(version.Length < 1) {
                return 0;
            }

            int dotPos = -1;
            if(version.Contains(".")) {
                dotPos = version.IndexOf(".");
            }

            string numbersOnly = "";
            for(int i = 0; i < version.Length; i++) {
                string thisChar = version.Substring(i, 1);
                int testIfInt = 0;
                if(int.TryParse(thisChar, out testIfInt)) {
                    numbersOnly += thisChar;
                }
            }

            if(dotPos > -1) {
                numbersOnly = numbersOnly.Insert(dotPos, ".");
            }

            numbersOnly = numbersOnly.Replace(",", ".");

            double resultVer = double.Parse(numbersOnly, CultureInfo.GetCultureInfo("en-US"));
            return resultVer;
        }

        private double DotnetRegistryVersion() {
            double cVersion = 0;
            double thisVersion = 0;

            // Opens the registry key for the .NET Framework entry. 
            using(RegistryKey ndpKey =
                RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "").
                OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\")) {

                foreach(string versionKeyName in ndpKey.GetSubKeyNames()) {
                    if(versionKeyName.StartsWith("v")) {

                        RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                        string name = (string)versionKey.GetValue("Version", "");
                        thisVersion = versionToDouble(name);
                        if(cVersion < thisVersion) {
                            cVersion = thisVersion;
                        }
                        string sp = versionKey.GetValue("SP", "").ToString();
                        string install = versionKey.GetValue("Install", "").ToString();

                        if(name != "") {
                            continue;
                        }

                        foreach(string subKeyName in versionKey.GetSubKeyNames()) {
                            RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                            name = (string)subKey.GetValue("Version", "");
                            thisVersion = versionToDouble(name);
                            if(cVersion < thisVersion) {
                                cVersion = thisVersion;
                            }
                        }

                    }
                }
            }
            return cVersion;
        }
    }
}
