using EasyChampionSelection.ECS;
using EasyChampionSelection.Helper_Windows;
using Hardcodet.Wpf.TaskbarNotification;
using RiotSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace EasyChampionSelection {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class wndMain : Window {

        #region Properties & Attributes

        private bool _manuallyEnableTimerVisual = false;
        private DispatcherTimer _tmrCheckForChampSelect = new DispatcherTimer();
        private ChampionList _allChampions;
        private wndClientOverload _wndCO;
        private wndContactCreator _wndCC;
        private wndSettings _wndST;
        private StaticGroupManager _gm;
        private Settings _ecsSettings;
        private TaskbarIcon _notifyIcon;
        private StaticLolClientGraphics _lcg;
        private TimeSpan _tmspTimerClienActiveInterval = new TimeSpan(500);
        private TimeSpan _tmspTimerAfkInterval = new TimeSpan(5000);
        #endregion Properties & Attributes

        public wndMain() {
            InitializeComponent();
        }

        #region Events

        private void frmMain_Loaded(object sender, RoutedEventArgs e) {
            LoadSettings(); //Load this first

            if(!_ecsSettings.ShowMainFormOnLaunch) {
                this.Visibility = Visibility.Hidden;
                this.ShowInTaskbar = false;
            }

            SetupNotifyIcon(); //TrayIcon

            LoadSerializedGroupManager();
            LoadAllChampions(); //Load all champions (Riot api or local)

            //Visualize the data
            DisplayGroups();
            DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();

            SetupTimer(); //Timer
        }

        private void frmMain_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            SaveSerializedData();

            _notifyIcon.Dispose(); //Dispose to auto clear the icon

            Application.Current.Shutdown();
        }

        private void _AllChampions_ChampionsChanged(ChampionList sender, EventArgs e) {
            DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
        }

        private void _gmGroupManager_GroupsChanged(StaticGroupManager sender, GroupManagerEventArgs e) {
            if(e.eventOperation == GroupManagerEventOperation.Add) {
                e.operationItem.NameChanged += _gmGroupManager_ChampionList_NameChanged;
            }
            DisplayGroups();
        }

        private void _gmGroupManager_ChampionList_NameChanged(ChampionList sender, EventArgs e) {
            DisplayGroups();
        }

        private void _ecsSettings_ClientOverlayChanged(Settings sender, EventArgs e) {
            if(_wndCO != null) {
                _wndCO.RepositionClientOverlay();
            }
        }

        private void _ecsSettings_ApiKeyChanged(Settings sender, EventArgs e) {
            LoadAllChampionsRiotApi();
        }

        private void lolClientHasClosed() {
            _tmrCheckForChampSelect.Interval = _tmspTimerAfkInterval;
            DeleteOldStaticLolClientGraphics(); //Delete all references to client dependent classes with events
            _tmrCheckForChampSelect.Start();
        }

        private void _tmrCheckForChampSelect_Tick(object sender, EventArgs e) {
            _tmrCheckForChampSelect.Stop();

            try {
                //Check if player is ingame
                Process[] gameClient = Process.GetProcessesByName("League of Legends");
                if(gameClient.Count() > 0) {
                    _tmrCheckForChampSelect.Interval = _tmspTimerAfkInterval;
                } else {
                    _tmrCheckForChampSelect.Interval = _tmspTimerClienActiveInterval;

                    Process[] p = Process.GetProcessesByName("lolclient"); //Look for the lolClient process
                    if(_lcg == null) {
                        if(p.Length > 0) {
                            NewStaticLolClientGraphics(p[0]);
                        } else {
                            lolClientHasClosed();
                            return;
                        }
                    }

                    if(p.Length == 0) {
                        lolClientHasClosed();
                        return;
                    }

                    bool clientExists = false;
                    for(int i = 0; i < p.Length; i++) {
                        if(p[i].Id == _lcg.getProcessLolClient().Id) {
                            clientExists = true;
                            break;
                        }
                    }

                    if(!clientExists) {
                        NewStaticLolClientGraphics(p[0]);
                    }

                    if(_lcg.isLolClientFocussed() || _lcg.isEasyChampionSelectionFoccussed()) {
                        if(_lcg.isInChampSelect()) {
                            _wndCO.Visibility = System.Windows.Visibility.Visible;
                        } else {
                            _wndCO.Visibility = System.Windows.Visibility.Hidden;
                        }
                    }

                }
            } catch(Exception ex) {
                SaveSerializedData();
                wndErrorHelper wndEH = new wndErrorHelper(ex);
                wndEH.Owner = this;
                wndEH.ShowDialog();
                _tmrCheckForChampSelect.Stop();
                _manuallyEnableTimerVisual = true;
                return;
            }
            _tmrCheckForChampSelect.Start();
        }

        private void btnGuide_Click(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo("https://github.com/BertGoens/EasyChampionSelection"));
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e) {
            if(_wndST != null) {
                if(_wndST.IsLoaded == false) {
                    _wndST = new wndSettings(_ecsSettings, _lcg);
                    _wndST.Owner = this;
                }
            } else {
                _wndST = new wndSettings(_ecsSettings, _lcg);
                _wndST.Owner = this;
            }

            _wndST.Show();
        }

        private void btnCredits_Click(object sender, RoutedEventArgs e) {
            try {
                wndCredits wndCR = new wndCredits();
                wndCR.Owner = this;
                wndCR.ShowDialog();
            } catch(Exception) {
                DisplayPopup("Woops, something went wrong!");
            }

        }

        private void btnNewGroup_Click(object sender, RoutedEventArgs e) {
            try {
                wndAddGroup wndAG = new wndAddGroup(_gm);
                wndAG.Owner = this;
                wndAG.ShowDialog();
            } catch(Exception) {
                DisplayPopup("Woops, something went wrong!");
            }
        }

        private void btnDeleteGroup_Click(object sender, RoutedEventArgs e) {
            if(lsbGroups.SelectedIndex > -1) {
                if(MessageBoxResult.Yes == MessageBox.Show("Remove: " + lsbGroups.SelectedItem.ToString(), "Remove group", MessageBoxButton.YesNo)) {
                    _gm.RemoveGroup(lsbGroups.SelectedIndex);
                    lblGroupsCount.Content = "Groups: " + lsbGroups.Items.Count + " / " + _gm.MaxGroups;
                    DisplayGroups();
                    DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
                }
            }
        }

        private void lsbChampionsInGroupItem_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            RemoveChampionsFromCurrentGroup(this, e);
        }

        private void lsbAllChampionsItem_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            AddChampionsToCurrentGroup(this, e);
        }

        private void lsbGroups_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            lsbChampionsInGroup.Items.Clear();
            DisplayChampsInSelectedGroup();
            DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
        }

        private void AddChampionsToCurrentGroup(object sender, RoutedEventArgs e) {
            for(int i = 0; i < lsbAllChampions.SelectedItems.Count; i++) {
                ListBoxItem lbi = (ListBoxItem)lsbAllChampions.SelectedItems[i];
                _gm.getGroup(lsbGroups.SelectedIndex).AddChampion(lbi.Content.ToString());
            }

            DisplayChampsInSelectedGroup();
            DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
        }

        private void RemoveChampionsFromCurrentGroup(object sender, RoutedEventArgs e) {
            for(int i = 0; i < lsbChampionsInGroup.SelectedItems.Count; i++) {
                ListBoxItem lbi = (ListBoxItem)lsbChampionsInGroup.SelectedItems[i];
                _gm.getGroup(lsbGroups.SelectedIndex).RemoveChampion(lbi.Content.ToString());
            }
            DisplayChampsInSelectedGroup();
            DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
        }

        private void txtFilterForAllChampions_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            lblFilterInfo.Visibility = System.Windows.Visibility.Hidden;
        }

        private void txtFilterForAllChampions_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            if(txtFilterForAllChampions.Text.Length < 1) {
                lblFilterInfo.Visibility = System.Windows.Visibility.Visible;
            } else {
                lblFilterInfo.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void txtFilterForAllChampions_TextChanged(object sender, TextChangedEventArgs e) {
            if(txtFilterForAllChampions.Text.Length < 1) {
                lblFilterInfo.Visibility = System.Windows.Visibility.Visible;
            } else {
                lblFilterInfo.Visibility = System.Windows.Visibility.Hidden;
            }
            DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
        }

        #region ContextMenus

        //Context Menu notifyIcon
        private void notifyIcon_PreviewTrayContextMenuOpen(object sender, System.Windows.RoutedEventArgs e) {
            _notifyIcon.ContextMenu = null;
            System.Windows.Controls.ContextMenu cm = new System.Windows.Controls.ContextMenu();

            if(this.Visibility == System.Windows.Visibility.Visible) {
                MenuItem mniHideMainWindow = CreateMenuItem("Hide Main window", mniHideMainWindow_Click);
                cm.Items.Add(mniHideMainWindow);
            } else {
                MenuItem mniShowMainWindow = CreateMenuItem("Show Main window", mniShowMainWindow_Click);
                cm.Items.Add(mniShowMainWindow);
            }

            cm.Items.Add(new Separator());
            if(_manuallyEnableTimerVisual) {
                MenuItem mniManuallyEnableTimer = CreateMenuItem("Start timer", mniManuallyEnableTimer_Click);
                cm.Items.Add(mniManuallyEnableTimer);
            }

            MenuItem mniContactCreator = CreateMenuItem("Contact creator", mniContactCreator_Click);
            cm.Items.Add(mniContactCreator);

            cm.Items.Add(new Separator());

            MenuItem mniExit = CreateMenuItem("Exit", mniExit_Click);
            cm.Items.Add(mniExit);

            _notifyIcon.ContextMenu = cm;
        }

        private void mniShowMainWindow_Click(object sender, RoutedEventArgs e) {
            this.Visibility = System.Windows.Visibility.Visible;
            this.ShowInTaskbar = true;
        }

        private void mniHideMainWindow_Click(object sender, RoutedEventArgs e) {
            this.Visibility = System.Windows.Visibility.Hidden;
            this.ShowInTaskbar = false;
        }

        private void mniManuallyEnableTimer_Click(object sender, RoutedEventArgs e) {
            _manuallyEnableTimerVisual = false;
            _tmrCheckForChampSelect.Start();
        }

        private void mniContactCreator_Click(object sender, RoutedEventArgs e) {
            if(_wndCC == null) {
                _wndCC = new wndContactCreator();
                _wndCC.Owner = this;
                _wndCC.Show();
            } else {
                _wndCC.Show();
            }
        }

        private void mniExit_Click(object sender, RoutedEventArgs e) {
            base.Close();
        }

        //Context Menu lsbGroups
        private void lsbGroups_PreviewMouseRightButtonDown(object sender, System.Windows.RoutedEventArgs e) {
            lsbGroups.ContextMenu = null;
            ContextMenu cm = new ContextMenu();

            MenuItem mniRenameGroup = CreateMenuItem("Rename Group", mniRenameGroup_Click);
            cm.Items.Add(mniRenameGroup);

            if(lsbGroups.SelectedItems.Count < 1) {
                mniRenameGroup.IsEnabled = false;
            }

            MenuItem mniNewGroup = CreateMenuItem("New Group", mniNewGroup_Click);
            cm.Items.Add(mniNewGroup);

            if(_gm.MaxGroups <= _gm.GroupCount) {
                mniNewGroup.IsEnabled = false;
            }

            MenuItem mniDeleteGroup = CreateMenuItem("Delete Group", mniDeleteGroup_Click);
            cm.Items.Add(mniDeleteGroup);

            if(lsbGroups.SelectedItems.Count != 1) {
                mniDeleteGroup.IsEnabled = false;
            }

            cm.Items.Add(new Separator());

            MenuItem mniMoveGroupUp = CreateMenuItem("Move Group Up", mniMoveGroupUp_Click);
            cm.Items.Add(mniMoveGroupUp);

            if(lsbGroups.SelectedIndex < 1) {
                mniMoveGroupUp.IsEnabled = false;
            }

            MenuItem mniMoveGroupDown = CreateMenuItem("Move Group Down", mniMoveGroupDown_Click);
            cm.Items.Add(mniMoveGroupDown);

            if(lsbGroups.SelectedItem == null || lsbGroups.SelectedIndex > lsbGroups.Items.Count - 2) {
                mniMoveGroupDown.IsEnabled = false;
            }

            lsbGroups.ContextMenu = cm;
            e.Handled = true;
        }

        void mniRenameGroup_Click(object sender, RoutedEventArgs e) {
            wndRenameGroup wndRG = new wndRenameGroup(_gm, lsbGroups.SelectedItem.ToString());
            wndRG.Owner = this;
            wndRG.ShowDialog();
        }

        void mniNewGroup_Click(object sender, RoutedEventArgs e) {
            btnNewGroup_Click(sender, e);
        }

        void mniDeleteGroup_Click(object sender, RoutedEventArgs e) {
            btnDeleteGroup_Click(sender, e);
        }

        void mniMoveGroupUp_Click(object sender, RoutedEventArgs e) {
            _gm.ReOrder((ChampionList)lsbGroups.SelectedItem, lsbGroups.SelectedIndex - 1);
            DisplayGroups();

        }

        void mniMoveGroupDown_Click(object sender, RoutedEventArgs e) {
            _gm.ReOrder((ChampionList)lsbGroups.SelectedItem, lsbGroups.SelectedIndex + 1);
            DisplayGroups();

        }

        //Context Menu lsbChampionsInGroup
        private void lsbChampionsInGroup_PreviewMouseRightButtonDown(object sender, System.Windows.RoutedEventArgs e) {
            lsbChampionsInGroup.ContextMenu = null;
            ContextMenu cm = new ContextMenu();

            MenuItem mniUnselectAll = CreateMenuItem("Unselect All", mniUnselectAll_Click);
            cm.Items.Add(mniUnselectAll);

            if(lsbGroups.SelectedItems.Count < 1) {
                mniUnselectAll.IsEnabled = false;
            }

            cm.Items.Add(new Separator());

            MenuItem mniRemove = CreateMenuItem("Remove", mniRemove_Click);
            cm.Items.Add(mniRemove);

            if(lsbGroups.SelectedItems.Count < 1) {
                mniRemove.IsEnabled = false;
            }

            lsbChampionsInGroup.ContextMenu = cm;
            e.Handled = true;
        }

        void mniUnselectAll_Click(object sender, RoutedEventArgs e) {
            lsbChampionsInGroup.UnselectAll();
        }

        void mniRemove_Click(object sender, RoutedEventArgs e) {
            RemoveChampionsFromCurrentGroup(sender, e);
        }

        //Context Menu lsbAllChampions
        private void lsbAllChampions_PreviewMouseRightButtonDown(object sender, System.Windows.RoutedEventArgs e) {
            lsbAllChampions.ContextMenu = null;
            ContextMenu cm = new ContextMenu();

            MenuItem mniUnselectAllChampions = CreateMenuItem("Unselect All", mniUnselectAllChampions_Click);
            cm.Items.Add(mniUnselectAllChampions);

            if(lsbGroups.SelectedItems.Count < 1) {
                mniUnselectAllChampions.IsEnabled = false;
            }

            cm.Items.Add(new Separator());

            MenuItem mniReloadWithApi = CreateMenuItem("Reload with Riot API", mniReloadWithApi_Click);
            cm.Items.Add(mniReloadWithApi);

            MenuItem mniReloadLocal = CreateMenuItem("Reload (local save)", mniReloadLocal_Click);
            cm.Items.Add(mniReloadLocal);

            MenuItem mniManuallyAddChampion = CreateMenuItem("Manually add a champion", mniManuallyAddChampion_Click);
            cm.Items.Add(mniManuallyAddChampion);
            
            lsbAllChampions.ContextMenu = cm;

            e.Handled = true;
        }

        private void mniUnselectAllChampions_Click(object sender, RoutedEventArgs e) {
            lsbAllChampions.UnselectAll();
        }

        private void mniReloadWithApi_Click(object sender, RoutedEventArgs e) {
            LoadAllChampionsRiotApi();
            DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
        }

        private void mniReloadLocal_Click(object sender, RoutedEventArgs e) {
            if(File.Exists(StaticSerializer.FullPath_AllChampions)) {
                LoadAllChampionsLocal();
                DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
            } else {
                MessageBox.Show("No local saves found!", this.Title);
            }
        }

        private void mniManuallyAddChampion_Click(object sender, RoutedEventArgs e) {
            wndAddChampion wndAC = new wndAddChampion(_allChampions);
            wndAC.Owner = this;
            wndAC.ShowDialog();
        }

        #endregion ContextMenus
        #endregion Events

        #region Private Behavior

        private MenuItem CreateMenuItem(String Header, RoutedEventHandler ClickEventHandler) {
            MenuItem mniMI = new MenuItem();
            mniMI.Header = Header;
            mniMI.Click += ClickEventHandler;
            return mniMI;
        }

        private void SetupNotifyIcon() {
            _notifyIcon = new TaskbarIcon();
            _notifyIcon.Icon = Properties.Resources.LolIcon;
            _notifyIcon.ToolTipText = "Easy Champion Selection";
            _notifyIcon.Visibility = Visibility.Visible;
            _notifyIcon.MenuActivation = PopupActivationMode.RightClick;
            System.Windows.Controls.ContextMenu cm = new System.Windows.Controls.ContextMenu();
            _notifyIcon.ContextMenu = cm;
            _notifyIcon.PreviewTrayContextMenuOpen += notifyIcon_PreviewTrayContextMenuOpen;
        }

        private void SetupTimer() {
            _tmrCheckForChampSelect.Tick += new EventHandler(_tmrCheckForChampSelect_Tick);
            _tmrCheckForChampSelect.Interval = _tmspTimerClienActiveInterval;
            _tmrCheckForChampSelect.Start();
        }

        private void LoadSettings() {
            if(File.Exists(StaticSerializer.FullPath_Settings)) {
                _ecsSettings = (Settings)StaticSerializer.DeSerializeObject(StaticSerializer.FullPath_Settings);
                if(_ecsSettings == null) {
                    _ecsSettings = new Settings();
                }
            } else {
                _ecsSettings = new Settings();
            }
            _ecsSettings.ClientOverlayChanged += _ecsSettings_ClientOverlayChanged;
            _ecsSettings.ApiKeyChanged += _ecsSettings_ApiKeyChanged;
        }

        private void LoadAllChampions() {
            _allChampions = new ChampionList("AllChampions");
            LoadAllChampionsRiotApi(); //Use api to get all champions
            if(_allChampions.getCount() < 1) {
                LoadAllChampionsLocal();
            }
            _allChampions.ChampionsChanged += _AllChampions_ChampionsChanged;
        }

        private async void LoadAllChampionsRiotApi() {
            DisplayPopup("Loading champions with API");
            _allChampions.RemoveAllChampions();
            if(_ecsSettings.UserApiKey.Length == 36) {
                try {
                    StaticRiotApi staticApi = StaticRiotApi.GetInstance(_ecsSettings.UserApiKey);
                    RiotSharp.StaticDataEndpoint.ChampionListStatic champions = await staticApi.GetChampionsAsync(RiotSharp.Region.euw, RiotSharp.StaticDataEndpoint.ChampionData.info, RiotSharp.Language.en_US);

                    for(int i = 0; i < champions.Champions.Count; i++) {
                        string ChampionName = champions.Champions.Values.ElementAt(i).Name;
                        _allChampions.AddChampion(ChampionName);
                    }

                } catch(RiotSharpException ex) {
                    DisplayPopup("Trouble loading trough the api: \n" + ex.ToString());
                } catch(NullReferenceException) {
                }
            } else {
                DisplayPopup("No correct API key found, get one at https://developer.riotgames.com/");
            }

        }

        private void LoadAllChampionsLocal() {
            if(File.Exists(StaticSerializer.FullPath_AllChampions)) {
                _allChampions = (ChampionList)StaticSerializer.DeSerializeObject(StaticSerializer.FullPath_AllChampions);
                DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
            } else {
                _allChampions = new ChampionList("AllChamps");
            }
        }

        private void LoadSerializedGroupManager() {
            lblCurrentGroupChampions.Content = "Create a group first.";

            if(File.Exists(StaticSerializer.FullPath_GroupManager)) {
                _gm = (StaticGroupManager)StaticSerializer.DeSerializeObject(StaticSerializer.FullPath_GroupManager);
                if(_gm != null) {
                    if(_gm.GroupCount > 0) {
                        lblCurrentGroupChampions.Content = "";
                        _gm.GroupsChanged += _gmGroupManager_GroupsChanged;
                        for(int i = 0; i < _gm.GroupCount; i++) {
                            _gm.getGroup(i).NameChanged += _gmGroupManager_ChampionList_NameChanged;
                        }
                    }
                } else {
                    NewGroupManager();
                }
            } else {
                NewGroupManager();
            }
        }

        private void NewGroupManager() {
            _gm = StaticGroupManager.GetInstance();
            _gm.GroupsChanged += _gmGroupManager_GroupsChanged;
        }

        private void DeleteOldStaticLolClientGraphics() {
            if(_wndCO != null) {
                _wndCO.SafeDelete(); //Unsubscribes from all events
                _wndCO = null;
            }

            _lcg = null;
        }

        private void NewStaticLolClientGraphics(Process LeagueOfLegendsClientProcess) {
            if(LeagueOfLegendsClientProcess != null) {
                DeleteOldStaticLolClientGraphics();

                _lcg = StaticLolClientGraphics.GetInstance(LeagueOfLegendsClientProcess, _ecsSettings);

                bool ReOpenConfigLolClientOverlay = false;
                bool settingsOpened = false;
                if(_wndST != null) {
                    settingsOpened = _wndST.IsLoaded;
                    if(_wndST.IsConfigLolClientOverlayOpened()) {
                        ReOpenConfigLolClientOverlay = true;
                    }
                    _wndST.SafeClose();
                }
                _wndST = new wndSettings(_ecsSettings, _lcg, ReOpenConfigLolClientOverlay);
                _wndST.Owner = this;
                if(settingsOpened) {
                    _wndST.Show();
                }

                _wndCO = new wndClientOverload(_gm, _lcg);

                DisplayPopup("Your lolclient.exe process just got updated.");
            }
        }

        private void SaveSerializedData() {
            StaticSerializer.SerializeObject(_gm, StaticSerializer.FullPath_GroupManager);
            StaticSerializer.SerializeObject(_allChampions, StaticSerializer.FullPath_AllChampions);
            StaticSerializer.SerializeObject(_ecsSettings, StaticSerializer.FullPath_Settings);
        }


        #endregion Private Behavior

        #region Public Behavior

        public void DisplayGroups() {
            lsbGroups.Items.Clear();
            for(int i = 0; i < _gm.GroupCount; i++) {
                lsbGroups.Items.Add(_gm.getGroup(i));
            }
            lblGroupsCount.Content = "Groups: " + lsbGroups.Items.Count + " / " + _gm.MaxGroups;
            if(_gm.GroupCount == _gm.MaxGroups) {
                btnNewGroup.IsEnabled = false;
            } else {
                btnNewGroup.IsEnabled = true;
            }
            if(_gm.GroupCount == 0) {
                btnDeleteGroup.IsEnabled = false;
            } else {
                btnDeleteGroup.IsEnabled = true;
                if(lsbGroups.SelectedItem == null) {
                    lsbGroups.SelectedIndex = 0;
                }
            }
        }

        public void DisplayChampsInSelectedGroup() {
            lsbChampionsInGroup.Items.Clear();
            if(lsbGroups.SelectedIndex > -1) {
                int championsInList = _gm.getGroup(lsbGroups.SelectedIndex).ChampionCount;
                for(int i = 0; i < championsInList; i++) {
                    ListBoxItem lsbChampionsInGroupItem = new ListBoxItem();
                    lsbChampionsInGroupItem.Content = _gm.getGroup(lsbGroups.SelectedIndex).getChampion(i);
                    lsbChampionsInGroupItem.PreviewMouseDoubleClick += lsbChampionsInGroupItem_PreviewMouseDoubleClick;
                    lsbChampionsInGroup.Items.Add(lsbChampionsInGroupItem);
                }
            }
            lblCurrentGroupChampions.Content = "Champions: " + lsbChampionsInGroup.Items.Count;
        }

        public void DisplayAllChampionsMinusInSelectedGroupAccordingToFilter() {
            lsbAllChampions.Items.Clear();
            if(lsbGroups.SelectedIndex > -1) {
                for(int i = 0; i < _allChampions.getCount(); i++) {
                    if(!_gm.getGroup(lsbGroups.SelectedIndex).Contains(_allChampions.getChampion(i))) { //lsbChampionsInGroup.Items.Contains(_allChampions.getChampion(i))
                        string newChamp = _allChampions.getChampion(i);
                        if(newChamp.ToUpper().Contains(txtFilterForAllChampions.Text.ToUpper())) {
                            ListBoxItem lsbAllChampionsItem = new ListBoxItem();
                            lsbAllChampionsItem.Content = _allChampions.getChampion(i);
                            lsbAllChampionsItem.PreviewMouseDoubleClick += lsbAllChampionsItem_PreviewMouseDoubleClick;
                            lsbAllChampions.Items.Add(lsbAllChampionsItem);
                        }
                    }
                }
            } else {
                for(int i = 0; i < _allChampions.getCount(); i++) {
                    string newChamp = _allChampions.getChampion(i);
                    if(newChamp.ToUpper().Contains(txtFilterForAllChampions.Text.ToUpper())) {
                        ListBoxItem lsbAllChampionsItem = new ListBoxItem();
                        lsbAllChampionsItem.Content = _allChampions.getChampion(i);
                        lsbAllChampionsItem.PreviewMouseDoubleClick += lsbAllChampionsItem_PreviewMouseDoubleClick;
                        lsbAllChampions.Items.Add(lsbAllChampionsItem);
                    }
                }
            }

            lblAllChampionsInfo.Content = "All champions: " + lsbAllChampions.Items.Count;
        }

        public void DisplayPopup(string message) {
            try {
                FancyBalloon balloon = new FancyBalloon(this.Title, message);
                _notifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Fade, 3500);
            } catch(Exception) { }
        }

        #endregion Public Behavior

    }
}