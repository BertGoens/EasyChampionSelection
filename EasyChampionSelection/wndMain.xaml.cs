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

/*
 * Authors:
 * Anymeese [NA]
 * Summoner
 * BertnFTW [EUW]
 * /u/Arkandos [Reddit]
 * 
 * In no specific order
 */

namespace EasyChampionSelection {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class wndMain : Window {

        #region Properties & Attributes

        /// <summary>
        /// Checks if the lolClient is still focussed
        /// </summary>
        private DispatcherTimer tmrCheckForChampSelect = new DispatcherTimer();
        /// <summary>
        /// A list of all Groups
        /// </summary>
        private List<string> lstAllGroups;
        /// <summary>
        /// A list of all Champions
        /// </summary>
        private List<string> lstAllChampions;
        /// <summary>
        /// The overlay window for lolClient.exe
        /// Should pop up every time it has focus!
        /// </summary>
        private wndClientOverload wndCO;
        private bool forceShowClientOverload;
        /// <summary>
        /// Each champion / group is stored in here
        /// </summary>
        public GroupManager gmGroupManager;
        /// <summary>
        /// Private RIOT API key, please don't abuse!
        /// </summary>
        private const string API_KEY = "fbd3429a-256e-405b-84e4-081afba9ba17";
        /// <summary>
        /// The taskbar notify icon
        /// </summary>
        private TaskbarIcon notifyIcon;
        /// <summary>
        /// A helper to determine the state of lolClient.exe
        /// </summary>
        public StaticLolClientGraphics _lolClientHelper;

        #endregion Properties & Attributes

        #region Constructor
        public wndMain() {
            lstAllGroups = new List<string>();
            lstAllChampions = new List<string>();

            InitializeComponent();
        }
        #endregion Constructor

        #region Getters & Setters

        public List<string> GetGroups() {
            return lstAllGroups;
        }

        public List<string> GetAllChampions() {
            return lstAllChampions;
        }

        private void SetLolClientHelper(Process LeagueOfLegendsClientProcess) {
            if(LeagueOfLegendsClientProcess != null) {
                _lolClientHelper = StaticLolClientGraphics.GetInstance(LeagueOfLegendsClientProcess);
                _lolClientHelper.OnLeagueClientReposition += wndCO.StaticLolClientGraphics_OnLeagueClientReposition;
                DisplayPopup("Your lolClient.exe process just got updated.");
            }
        }

        #endregion Getters & Setters

        #region Events

        private void frmMain_Loaded(object sender, RoutedEventArgs e) {
            //TrayIcon
            SetupNotifyIcon();

            //Use api to get all champions
            LoadAllChampionsRiotApi();
            if(lstAllChampions.Count < 1) {
                LoadAllChampionsLocal();
            }

            //Helper window
            wndCO = new wndClientOverload(this);
            wndCO.Owner = this;
            
            //Load Serialized GroupManager_Groups serialized data
            LoadSerializedGroupManager();

            //Visualize the data
            wndCO.Redraw();
            DisplayGroups();
            DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();

            //Timer
            tmrCheckForChampSelect.Tick += new EventHandler(tmrCheckForChampSelect_Tick);
            tmrCheckForChampSelect.Interval = new TimeSpan(500);
            tmrCheckForChampSelect.IsEnabled = true;
        }

        private void frmMain_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            //Important: unsubscribe from events to not serializable stuff (etc windows)
            gmGroupManager.GroupsChanged -= wndCO.GroupManager_GroupsChanged;
            for(int i = 0; i < gmGroupManager.GroupCount; i++) {
                gmGroupManager.getGroup(i).NameChanged -= wndCO.ChampionList_NameChanged;
            }

            //Serialize important stuff
            StaticSerializer.SerializeObject(gmGroupManager, StaticSerializer.PATH_GroupManager);
            if(lstAllChampions.Count > 120) {
                StaticSerializer.SerializeObject(lstAllChampions, StaticSerializer.PATH_AllChampions);
            }
            notifyIcon.Dispose();
        }

        private void tmrCheckForChampSelect_Tick(object sender, EventArgs e) {
            tmrCheckForChampSelect.Stop();

            //Look for the lolClient process
            Process[] p = Process.GetProcessesByName("LolClient");

            if(_lolClientHelper != null) {
                //Check if the lolClient process hasn't changed (ie: close & restart)
                if(p.Count() > 0) {
                    if(p[0].Id == _lolClientHelper.GetProcessLolClient().Id) { //Is same client?
                        if(_lolClientHelper.isLolClientFocussed() || _lolClientHelper.isEasyChampionSelectionFoccussed()) { //If lolclient or ECS = active window
                            if(_lolClientHelper.isInChampSelect()) { //Is it in champion select?
                                if(wndCO.Visibility != System.Windows.Visibility.Visible) { //Is the ClientOverlay visible?
                                    wndCO.Show();
                                }
                            } else { // not in champ select, hide overlay
                                wndCO.Visibility = System.Windows.Visibility.Hidden;
                            }
                        } else { // lolclient not foccussed
                            if(!forceShowClientOverload) {
                                wndCO.Visibility = System.Windows.Visibility.Hidden;
                            }
                        }
                    } else { //Create new / update lolClientHelper
                        SetLolClientHelper(p[0]);
                    }
                }
            } else {
                if(p.Count() > 0) {
                    SetLolClientHelper(p[0]);
                }
            }

            tmrCheckForChampSelect.Start();
        }

        private void btnCredits_Click(object sender, RoutedEventArgs e) {
            wndCredits wndCR = new wndCredits();
            wndCR.Owner = this;
            wndCR.Show();
        }

        private void btnNewGroup_Click(object sender, RoutedEventArgs e) {
            wndAddGroup wndAG = new wndAddGroup(this);
            wndAG.Owner = this;
            wndAG.ShowDialog();
        }

        private void btnDeleteGroup_Click(object sender, RoutedEventArgs e) {
            if(lsbGroups.SelectedIndex > -1) {
                if(MessageBoxResult.Yes == MessageBox.Show("Remove: " + lsbGroups.SelectedItem.ToString(), "Remove group", MessageBoxButton.YesNo)) {
                    gmGroupManager.RemoveGroup(lsbGroups.SelectedIndex);
                    lblGroupsCount.Content = "Groups: " + lsbGroups.Items.Count + " / " + gmGroupManager.MaxGroups;
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
                gmGroupManager.getGroup(lsbGroups.SelectedIndex).AddChampion(lsbAllChampions.SelectedItems[i].ToString());
            }

            DisplayChampsInSelectedGroup();
            DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
        }

        private void btnRemoveChampionsFromCurrentGroup_Click(object sender, RoutedEventArgs e) {
            for(int i = 0; i < lsbChampionsInGroup.SelectedItems.Count; i++) {
                gmGroupManager.getGroup(lsbGroups.SelectedIndex).RemoveChampion(lsbChampionsInGroup.SelectedItems[i].ToString());
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
            notifyIcon.ContextMenu = null;
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

            if(wndCO.Visibility != System.Windows.Visibility.Visible) {
                mniHide.IsEnabled = false;
            } else {
                mniShow.IsEnabled = false;
            }

            cm.Items.Add(new Separator());

            MenuItem mniExit = CreateMenuItem("Exit", mniExit_Click);
            cm.Items.Add(mniExit);

            notifyIcon.ContextMenu = cm;
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
            wndCO.Show();
            forceShowClientOverload = true;
        }

        private void mniHide_Click(object sender, RoutedEventArgs e) {
            wndCO.Visibility = System.Windows.Visibility.Hidden;
            forceShowClientOverload = false;
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

            if(gmGroupManager.MaxGroups <= gmGroupManager.GroupCount) {
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

            if(lsbGroups.SelectedItem == null || lsbGroups.SelectedIndex > lsbGroups.Items.Count -2) {
                mniMoveGroupDown.IsEnabled = false;
            }

            lsbGroups.ContextMenu = cm;
            e.Handled = true;
        }

        void mniRenameGroup_Click(object sender, RoutedEventArgs e) {
            wndRenameGroup wndRG = new wndRenameGroup(this, lsbGroups.SelectedItem.ToString());
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
            gmGroupManager.ReOrder((ChampionList)lsbGroups.SelectedItem, lsbGroups.SelectedIndex - 1);
            DisplayGroups();

        }

        void mniMoveGroupDown_Click(object sender, RoutedEventArgs e) {
            gmGroupManager.ReOrder((ChampionList)lsbGroups.SelectedItem, lsbGroups.SelectedIndex + 1);
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
                    if(!lstAllChampions.Contains(champsInClipboardText[i])) {
                        lstAllChampions.Add(champsInClipboardText[i]);
                        gmGroupManager.getGroup(lsbGroups.SelectedIndex).AddChampion(champsInClipboardText[i]);
                    } else if(!gmGroupManager.getGroup(lsbGroups.SelectedIndex).Contains(champsInClipboardText[i])) {
                        gmGroupManager.getGroup(lsbGroups.SelectedIndex).AddChampion(champsInClipboardText[i]);
                    }
                }
            }
            lstAllChampions.Sort();
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
            wndAddChampion wndAC = new wndAddChampion(this);
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
                if(!lstAllChampions.Contains(champsInClipboardText[i])) {
                    if(champsInClipboardText[i].Length > 1) {
                        lstAllChampions.Add(champsInClipboardText[i]);
                    }
                }
            }
            lstAllChampions.Sort();
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
            notifyIcon = new TaskbarIcon();
            notifyIcon.Icon = Properties.Resources.LolIcon;
            notifyIcon.ToolTipText = "Easy Champion Selection";
            notifyIcon.Visibility = Visibility.Visible;
            notifyIcon.MenuActivation = PopupActivationMode.RightClick;
            System.Windows.Controls.ContextMenu cm = new System.Windows.Controls.ContextMenu();
            notifyIcon.ContextMenu = cm;
            notifyIcon.PreviewTrayContextMenuOpen += notifyIcon_PreviewTrayContextMenuOpen;
        }

        private void LoadAllChampionsRiotApi() {
            lstAllChampions.Clear();
            try {
                var staticApi = StaticRiotApi.GetInstance(API_KEY);
                var champions = staticApi.GetChampions(RiotSharp.Region.euw, RiotSharp.StaticDataEndpoint.ChampionData.info, RiotSharp.Language.en_US);

                for(int i = 0; i < champions.Champions.Count; i++) {
                    string ChampionName = champions.Champions.Values.ElementAt(i).Name;
                    lstAllChampions.Add(ChampionName);
                }

                lstAllChampions.Sort();
            } catch(RiotSharpException ex) {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            } catch(NullReferenceException ex) {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        private void LoadAllChampionsLocal() {
            if(File.Exists(StaticSerializer.PATH_AllChampions)) {
                lstAllChampions = (List<string>)StaticSerializer.DeSerializeObject(StaticSerializer.PATH_AllChampions);
                DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
            }
        }

        private void LoadSerializedGroupManager() {
            if(File.Exists(StaticSerializer.PATH_GroupManager)) {
                gmGroupManager = (GroupManager)StaticSerializer.DeSerializeObject(StaticSerializer.PATH_GroupManager);
                if(gmGroupManager != null) {
                    if(gmGroupManager.GroupCount > 0) {
                        gmGroupManager.GroupsChanged += wndCO.GroupManager_GroupsChanged;
                        for(int i = 0; i < gmGroupManager.GroupCount; i++) {
                            gmGroupManager.getGroup(i).NameChanged += wndCO.ChampionList_NameChanged;
                        }
                        lsbGroups.SelectedIndex = 0;
                    } else {
                        lblCurrentGroupChampions.Content = "Create a group first.";
                    }
                } else {
                    gmGroupManager = new GroupManager();
                    gmGroupManager.GroupsChanged += wndCO.GroupManager_GroupsChanged;
                    lblCurrentGroupChampions.Content = "Create a group first.";
                }
            } else {
                gmGroupManager = new GroupManager();
                gmGroupManager.GroupsChanged += wndCO.GroupManager_GroupsChanged;
                lblCurrentGroupChampions.Content = "Create a group first.";
            }
        }

        #endregion Private Behavior

        #region Public Behavior

        public void AddGroup(string name) {
            ChampionList cList = new ChampionList(name);
            cList.NameChanged += wndCO.ChampionList_NameChanged;
            gmGroupManager.AddGroup(cList);
            DisplayGroups();
        }

        public void AddChampion(string name) {
            bool isNewChamp = true;

            for(int i = 0; i < lstAllChampions.Count; i++) {
                if(lstAllChampions[i] == name) {
                    isNewChamp = false;
                }
            }

            if(isNewChamp) {
                lstAllChampions.Add(name);
                lstAllChampions.Sort();
            }
        }

        public void DisplayGroups() {
            lsbGroups.Items.Clear();
            for(int i = 0; i < gmGroupManager.GroupCount; i++) {
                lsbGroups.Items.Add(gmGroupManager.getGroup(i));
            }
            lblGroupsCount.Content = "Groups: " + lsbGroups.Items.Count + " / " + gmGroupManager.MaxGroups;
            if(gmGroupManager.GroupCount == gmGroupManager.MaxGroups) {
                btnNewGroup.IsEnabled = false;
            } else {
                btnNewGroup.IsEnabled = true;
            }
            if(gmGroupManager.GroupCount == 0) {
                btnDeleteGroup.IsEnabled = false;
            } else {
                btnDeleteGroup.IsEnabled = true;
            }
        }

        public void DisplayChampsInSelectedGroup() {
            lsbChampionsInGroup.Items.Clear();
            if(lsbGroups.SelectedIndex > -1) {
                int championsInList = gmGroupManager.getGroup(lsbGroups.SelectedIndex).ChampionCount;
                for(int i = 0; i < championsInList; i++) {
                    lsbChampionsInGroup.Items.Add(gmGroupManager.getGroup(lsbGroups.SelectedIndex).getChampion(i));
                }
            }
            lblCurrentGroupChampions.Content = "Champions: " + lsbChampionsInGroup.Items.Count;
        }

        public void DisplayAllChampionsMinusInSelectedGroupAccordingToFilter() {
            lsbAllChampions.Items.Clear();
            for(int i = 0; i < lstAllChampions.Count; i++) {
                if(!lsbChampionsInGroup.Items.Contains(lstAllChampions[i])) {
                    string newChamp = lstAllChampions[i];
                    if(newChamp.ToUpper().Contains(txtFilterForAllChampions.Text.ToUpper())) {
                        lsbAllChampions.Items.Add(lstAllChampions[i]);
                    }
                }
            }
            lblAllChampionsInfo.Content = "All champions: " + lsbAllChampions.Items.Count;
        }

        public void DisplayPopup(string message) {
            try {
                FancyBalloon balloon = new FancyBalloon(this.Title, message);
                notifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Fade, 3500);
            } catch(Exception) { }
        }

        #endregion Public Behavior

    }
}