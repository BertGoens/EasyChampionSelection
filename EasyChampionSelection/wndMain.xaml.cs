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

        private DispatcherTimer _tmrCheckForChampSelect = new DispatcherTimer();
        private ChampionList _allChampions;
        private wndClientOverload _wndCO;
        private wndConfigLolClientOverlay _wndCLCO;
        private bool _forceShowClientOverload;
        public StaticGroupManager _gmGroupManager;
        private Settings _ecsSettings;
        private TaskbarIcon _notifyIcon;
        public StaticLolClientGraphics _lolClientHelper;

        #endregion Properties & Attributes

        public wndMain() {
            InitializeComponent();
        }

        #region Events

        private void frmMain_Loaded(object sender, RoutedEventArgs e) {
            LoadSettings(); //Load this first

            if(!_ecsSettings.ShowMainFormOnLaunch) {
                this.Visibility = System.Windows.Visibility.Hidden;
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
            //Important: unsubscribe from events to not serializable stuff (etc windows)
            _allChampions.ChampionsChanged -= _AllChampions_ChampionsChanged;
            _gmGroupManager.GroupsChanged -= _gmGroupManager_GroupsChanged;
            for(int i = 0; i < _gmGroupManager.GroupCount; i++) {
                _gmGroupManager.getGroup(i).NameChanged -= _wndCO.ChampionList_NameChanged;
            }

            //Serialize important stuff
            StaticSerializer.SerializeObject(_gmGroupManager, StaticSerializer.PATH_FolderForSaveData + StaticSerializer.PATH_GroupManager);
            if(_allChampions.getCount() > 120) {
                StaticSerializer.SerializeObject(_allChampions, StaticSerializer.PATH_FolderForSaveData + StaticSerializer.PATH_AllChampions);
            }
            StaticSerializer.SerializeObject(_ecsSettings, StaticSerializer.PATH_FolderForSaveData + StaticSerializer.PATH_Settings);

            _notifyIcon.Dispose(); //Dispose to auto clear the icon
        }

        private void _AllChampions_ChampionsChanged(ChampionList sender, EventArgs e) {
            DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
        }

        private void _gmGroupManager_GroupsChanged(StaticGroupManager sender, GroupManagerEventArgs e) {
            DisplayGroups();
            _wndCO.GroupManager_GroupsChanged(sender, e);
        }

        private void tmrCheckForChampSelect_Tick(object sender, EventArgs e) {
            _tmrCheckForChampSelect.Stop();

            //Look for the lolClient process
            Process[] p = Process.GetProcessesByName("LolClient");

            if(_lolClientHelper != null) {
                //Check if the lolClient process hasn't changed (ie: close & restart)
                if(p.Count() > 0) {
                    if(p[0].Id == _lolClientHelper.GetProcessLolClient().Id) { //Is same client?
                        if(_lolClientHelper.isLolClientFocussed() || _lolClientHelper.isEasyChampionSelectionFoccussed()) { //If lolclient or ECS = active window
                            if(_lolClientHelper.isInChampSelect()) { //Is it in champion select?
                                if(_wndCO.Visibility != System.Windows.Visibility.Visible) { //Is the ClientOverlay visible?
                                    _wndCO.Show();
                                }
                            } else { // not in champ select, hide overlay
                                _wndCO.Visibility = System.Windows.Visibility.Hidden;
                            }
                        } else { // lolclient not foccussed
                            if(!_forceShowClientOverload) {
                                _wndCO.Visibility = System.Windows.Visibility.Hidden;
                            }
                        }
                    } else { //Create new / update lolClientHelper
                        NewStaticLolClientGraphics(p[0]);
                    }
                }
            } else {
                if(p.Count() > 0) {
                    NewStaticLolClientGraphics(p[0]);
                }
            }

            _tmrCheckForChampSelect.Start();
        }

        private void btnConfigClientOverlay_Click(object sender, RoutedEventArgs e) {
            bool showError = true;

            if(_lolClientHelper != null) {
                if(_lolClientHelper.GetProcessLolClient() != null) {
                    try {
                        showError = false;
                        _wndCLCO.Owner = this;
                        _wndCLCO.Show();
                    } catch(Exception) {
                        DisplayPopup("Woops, something went wrong!");
                    }

                }
            }

            if(showError) {
                DisplayPopup("Please start league of legends first so I can take a screenshot!");
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e) {
            try {
                wndSettings wndST = new wndSettings(_ecsSettings);
                wndST.Owner = this;
                wndST.ShowDialog();
            } catch(Exception) {
                DisplayPopup("Woops, something went wrong!");
            }

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
                wndAddGroup wndAG = new wndAddGroup(_gmGroupManager);
                wndAG.Owner = this;
                wndAG.ShowDialog();
            } catch(Exception) {
                DisplayPopup("Woops, something went wrong!");
            }
        }

        private void btnDeleteGroup_Click(object sender, RoutedEventArgs e) {
            if(lsbGroups.SelectedIndex > -1) {
                if(MessageBoxResult.Yes == MessageBox.Show("Remove: " + lsbGroups.SelectedItem.ToString(), "Remove group", MessageBoxButton.YesNo)) {
                    _gmGroupManager.RemoveGroup(lsbGroups.SelectedIndex);
                    lblGroupsCount.Content = "Groups: " + lsbGroups.Items.Count + " / " + _gmGroupManager.MaxGroups;
                    DisplayGroups();
                    DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
                }
            }
        }

        private void lsbGroups_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            lsbChampionsInGroup.Items.Clear();
            DisplayChampsInSelectedGroup();
            DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
        }

        private void btnAddChampionsToCurrentGroup_Click(object sender, RoutedEventArgs e) {
            for(int i = 0; i < lsbAllChampions.SelectedItems.Count; i++) {
                _gmGroupManager.getGroup(lsbGroups.SelectedIndex).AddChampion(lsbAllChampions.SelectedItems[i].ToString());
            }

            DisplayChampsInSelectedGroup();
            DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
        }

        private void btnRemoveChampionsFromCurrentGroup_Click(object sender, RoutedEventArgs e) {
            for(int i = 0; i < lsbChampionsInGroup.SelectedItems.Count; i++) {
                _gmGroupManager.getGroup(lsbGroups.SelectedIndex).RemoveChampion(lsbChampionsInGroup.SelectedItems[i].ToString());
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

            MenuItem mniShow = CreateMenuItem("Show Client Overlay", mniShow_Click);
            cm.Items.Add(mniShow);

            MenuItem mniHide = CreateMenuItem("Hide Client Overlay", mniHide_Click);
            cm.Items.Add(mniHide);

            if(_wndCO.Visibility != System.Windows.Visibility.Visible) {
                mniHide.IsEnabled = false;
            } else {
                mniShow.IsEnabled = false;
            }

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

        private void mniShow_Click(object sender, RoutedEventArgs e) {
            _wndCO.Show();
            _forceShowClientOverload = true;
        }

        private void mniHide_Click(object sender, RoutedEventArgs e) {
            _wndCO.Visibility = System.Windows.Visibility.Hidden;
            _forceShowClientOverload = false;
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

            if(_gmGroupManager.MaxGroups <= _gmGroupManager.GroupCount) {
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
            wndRenameGroup wndRG = new wndRenameGroup(_gmGroupManager, lsbGroups.SelectedItem.ToString());
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
            _gmGroupManager.ReOrder((ChampionList)lsbGroups.SelectedItem, lsbGroups.SelectedIndex - 1);
            DisplayGroups();

        }

        void mniMoveGroupDown_Click(object sender, RoutedEventArgs e) {
            _gmGroupManager.ReOrder((ChampionList)lsbGroups.SelectedItem, lsbGroups.SelectedIndex + 1);
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

            cm.Items.Add(new Separator());

            MenuItem mniCopy = CreateMenuItem("Copy", mniCopy_Click);
            cm.Items.Add(mniCopy);

            if(lsbGroups.SelectedItems.Count < 1) {
                mniCopy.IsEnabled = false;
            }

            MenuItem mniPaste = CreateMenuItem("Paste", mniPaste_Click);
            cm.Items.Add(mniPaste);

            if(lsbGroups.SelectedItems.Count < 1) {
                mniPaste.IsEnabled = false;
            }

            lsbChampionsInGroup.ContextMenu = cm;
            e.Handled = true;
        }

        void mniUnselectAll_Click(object sender, RoutedEventArgs e) {
            lsbChampionsInGroup.UnselectAll();
        }

        void mniRemove_Click(object sender, RoutedEventArgs e) {
            btnRemoveChampionsFromCurrentGroup_Click(sender, e);
        }

        void mniCopy_Click(object sender, RoutedEventArgs e) {
            string clipboardText = "";

            for(int i = 0; i < lsbChampionsInGroup.SelectedItems.Count; i++) {
                clipboardText += lsbChampionsInGroup.SelectedItems[i] + Environment.NewLine;

            }
            Clipboard.SetText(clipboardText, TextDataFormat.Text);
        }

        void mniPaste_Click(object sender, RoutedEventArgs e) {
            string clipboardText = "";
            clipboardText = Clipboard.GetText(TextDataFormat.Text);

            string[] champsInClipboardText = clipboardText.Split(Environment.NewLine.ToCharArray());

            for(int i = 0; i < champsInClipboardText.Length; i++) {
                if(champsInClipboardText[i].Length > 1) {
                    if(!_allChampions.Contains(champsInClipboardText[i])) {
                        _allChampions.AddChampion(champsInClipboardText[i]);
                        _gmGroupManager.getGroup(lsbGroups.SelectedIndex).AddChampion(champsInClipboardText[i]);
                    } else if(!_gmGroupManager.getGroup(lsbGroups.SelectedIndex).Contains(champsInClipboardText[i])) {
                        _gmGroupManager.getGroup(lsbGroups.SelectedIndex).AddChampion(champsInClipboardText[i]);
                    }
                }
            }
            DisplayChampsInSelectedGroup();
            DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
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

            cm.Items.Add(new Separator());

            MenuItem mniCopyAllChampions = CreateMenuItem("Copy", mniCopyAllChampions_Click);
            cm.Items.Add(mniCopyAllChampions);

            if(lsbAllChampions.SelectedItems.Count < 1) {
                mniCopyAllChampions.IsEnabled = false;
            }

            MenuItem mniPasteAllChampions = CreateMenuItem("Paste", mniPasteAllChampions_Click);
            cm.Items.Add(mniPasteAllChampions);

            if(lsbAllChampions.SelectedItems.Count < 1) {
                mniPasteAllChampions.IsEnabled = false;
            }

            lsbAllChampions.ContextMenu = cm;
        }

        void mniUnselectAllChampions_Click(object sender, RoutedEventArgs e) {
            lsbAllChampions.UnselectAll();
        }

        void mniReloadWithApi_Click(object sender, RoutedEventArgs e) {
            LoadAllChampionsRiotApi();
            DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
        }

        void mniReloadLocal_Click(object sender, RoutedEventArgs e) {
            if(File.Exists(StaticSerializer.PATH_AllChampions)) {
                LoadAllChampionsLocal();
                DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
            } else {
                MessageBox.Show("No local saves found!", this.Title);
            }
        }

        void mniManuallyAddChampion_Click(object sender, RoutedEventArgs e) {
            wndAddChampion wndAC = new wndAddChampion(_allChampions);
            wndAC.Owner = this;
            wndAC.ShowDialog();
        }

        void mniCopyAllChampions_Click(object sender, RoutedEventArgs e) {
            string clipboardText = "";

            for(int i = 0; i < lsbAllChampions.SelectedItems.Count; i++) {
                clipboardText += lsbAllChampions.SelectedItems[i] + Environment.NewLine;

            }
            Clipboard.SetText(clipboardText, TextDataFormat.Text);
        }

        void mniPasteAllChampions_Click(object sender, RoutedEventArgs e) {
            string clipboardText = "";
            clipboardText = Clipboard.GetText(TextDataFormat.Text);

            string[] champsInClipboardText = clipboardText.Split(Environment.NewLine.ToCharArray());

            for(int i = 0; i < champsInClipboardText.Length; i++) {
                if(!_allChampions.Contains(champsInClipboardText[i])) {
                    if(champsInClipboardText[i].Length > 1) {
                        _allChampions.AddChampion(champsInClipboardText[i]);
                    }
                }
            }
            DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
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
            _tmrCheckForChampSelect.Tick += new EventHandler(tmrCheckForChampSelect_Tick);
            _tmrCheckForChampSelect.Interval = new TimeSpan(500);
            _tmrCheckForChampSelect.IsEnabled = true;
        }

        private void LoadSettings() {
            if(File.Exists(StaticSerializer.PATH_FolderForSaveData + StaticSerializer.PATH_Settings)) {
                _ecsSettings = (Settings)StaticSerializer.DeSerializeObject(StaticSerializer.PATH_FolderForSaveData + StaticSerializer.PATH_Settings);
                if(_ecsSettings == null) {
                    _ecsSettings = new Settings();
                }
            } else {
                _ecsSettings = new Settings();
            }
        }

        private void LoadAllChampions() {
            LoadAllChampionsRiotApi(); //Use api to get all champions
            if(_allChampions.getCount() < 1) {
                LoadAllChampionsLocal();
            }
            _allChampions.ChampionsChanged += _AllChampions_ChampionsChanged;
        }

        private void LoadAllChampionsRiotApi() {
            _allChampions.RemoveAllChampions();
            if(_ecsSettings.UserApiKey.Length > 0) {
                try {
                    StaticRiotApi staticApi = StaticRiotApi.GetInstance(_ecsSettings.UserApiKey);
                    RiotSharp.StaticDataEndpoint.ChampionListStatic champions = staticApi.GetChampions(RiotSharp.Region.euw, RiotSharp.StaticDataEndpoint.ChampionData.info, RiotSharp.Language.en_US);

                    for(int i = 0; i < champions.Champions.Count; i++) {
                        string ChampionName = champions.Champions.Values.ElementAt(i).Name;
                        _allChampions.AddChampion(ChampionName);
                    }

                } catch(RiotSharpException ex) {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                } catch(NullReferenceException ex) {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            }

        }

        private void LoadAllChampionsLocal() {
            if(File.Exists(StaticSerializer.PATH_FolderForSaveData + StaticSerializer.PATH_AllChampions)) {
                _allChampions = (ChampionList)StaticSerializer.DeSerializeObject(StaticSerializer.PATH_FolderForSaveData + StaticSerializer.PATH_AllChampions);
                DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
            } else {
                _allChampions = new ChampionList("AllChamps");
            }
        }

        private void LoadSerializedGroupManager() {
            lblCurrentGroupChampions.Content = "Create a group first.";
            if(File.Exists(StaticSerializer.PATH_FolderForSaveData + StaticSerializer.PATH_GroupManager)) {
                _gmGroupManager = (StaticGroupManager)StaticSerializer.DeSerializeObject(StaticSerializer.PATH_FolderForSaveData + StaticSerializer.PATH_GroupManager);
                if(_gmGroupManager != null) {
                    if(_gmGroupManager.GroupCount > 0) {
                        lblCurrentGroupChampions.Content = "";
                        _gmGroupManager.GroupsChanged += _gmGroupManager_GroupsChanged;
                        for(int i = 0; i < _gmGroupManager.GroupCount; i++) {
                            _gmGroupManager.getGroup(i).NameChanged += _wndCO.ChampionList_NameChanged;
                        }
                        lsbGroups.SelectedIndex = 0;
                    }
                } else {
                    NewGroupManager();
                }
            } else {
                NewGroupManager();
            }
        }

        private void NewGroupManager() {
            _gmGroupManager = StaticGroupManager.GetInstance();
            _gmGroupManager.GroupsChanged += _gmGroupManager_GroupsChanged;
        }

        private void NewStaticLolClientGraphics(Process LeagueOfLegendsClientProcess) {
            if(LeagueOfLegendsClientProcess != null) {
                _lolClientHelper = StaticLolClientGraphics.GetInstance(LeagueOfLegendsClientProcess);
                _lolClientHelper.OnLeagueClientReposition += _wndCO.StaticLolClientGraphics_OnLeagueClientReposition;
                if(_wndCLCO != null) {
                    if(_wndCLCO.IsVisible == true) {
                        _wndCLCO.Close();
                    }
                }
                _wndCLCO = new wndConfigLolClientOverlay(_lolClientHelper, _ecsSettings);
                _wndCLCO.Owner = this;
                _wndCO = new wndClientOverload(_gmGroupManager, _ecsSettings, _lolClientHelper); //Helper window
                _wndCO.Owner = this;
                DisplayPopup("Your lolClient.exe process just got updated.");
            }
        }

        #endregion Private Behavior

        #region Public Behavior

        public void DisplayGroups() {
            lsbGroups.Items.Clear();
            for(int i = 0; i < _gmGroupManager.GroupCount; i++) {
                lsbGroups.Items.Add(_gmGroupManager.getGroup(i));
            }
            lblGroupsCount.Content = "Groups: " + lsbGroups.Items.Count + " / " + _gmGroupManager.MaxGroups;
            if(_gmGroupManager.GroupCount == _gmGroupManager.MaxGroups) {
                btnNewGroup.IsEnabled = false;
            } else {
                btnNewGroup.IsEnabled = true;
            }
            if(_gmGroupManager.GroupCount == 0) {
                btnDeleteGroup.IsEnabled = false;
            } else {
                btnDeleteGroup.IsEnabled = true;
            }
        }

        public void DisplayChampsInSelectedGroup() {
            lsbChampionsInGroup.Items.Clear();
            if(lsbGroups.SelectedIndex > -1) {
                int championsInList = _gmGroupManager.getGroup(lsbGroups.SelectedIndex).ChampionCount;
                for(int i = 0; i < championsInList; i++) {
                    lsbChampionsInGroup.Items.Add(_gmGroupManager.getGroup(lsbGroups.SelectedIndex).getChampion(i));
                }
            }
            lblCurrentGroupChampions.Content = "Champions: " + lsbChampionsInGroup.Items.Count;
        }

        public void DisplayAllChampionsMinusInSelectedGroupAccordingToFilter() {
            lsbAllChampions.Items.Clear();
            for(int i = 0; i < _allChampions.getCount(); i++) {
                if(!lsbChampionsInGroup.Items.Contains(_allChampions.getChampion(i))) {
                    string newChamp = _allChampions.getChampion(i);
                    if(newChamp.ToUpper().Contains(txtFilterForAllChampions.Text.ToUpper())) {
                        lsbAllChampions.Items.Add(_allChampions.getChampion(i));
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