using EasyChampionSelection.ECS;
using EasyChampionSelection.ECS.AppRuntimeResources;
using EasyChampionSelection.ECS.AppRuntimeResources.LolClient;
using EasyChampionSelection.ECS.RiotGameData;
using EasyChampionSelection.ECS.RiotGameData.GroupManager;
using EasyChampionSelection.Helper_Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace EasyChampionSelection {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class wndMain : Window {

        #region Properties & Attributes

        private AppRuntimeResourcesManager _ARR;
        private wndSettings _wndST;

        #endregion Properties & Attributes

        private wndMain() {
            InitializeComponent();
        }

        public wndMain(AppRuntimeResourcesManager arr) : this() {
            _ARR = arr;

            //Visualize the data
            DisplayGroups();
            DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();

            _ARR.AllChampions.ChampionsChanged += AllChampions_ChampionsChanged;
            _ARR.MyGroupManager.GroupsChanged += MyGroupManager_GroupsChanged;
            for(int i = 0; i < _ARR.MyGroupManager.GroupCount; i++) {
                _ARR.MyGroupManager.getGroup(i).NameChanged += MyGroupManager_ChampionList_NameChanged;
            }
            _ARR.MySettings.ApiKeyChanged += MySettings_ApiKeyChanged;
        }

        #region UI Events

        private void AllChampions_ChampionsChanged(ChampionList sender, EventArgs e) {
            DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
        }

        private void MyGroupManager_GroupsChanged(StaticGroupManager sender, GroupManagerEventArgs e) {
            if(e.eventOperation == GroupManagerEventOperation.Add) {
                e.operationItem.NameChanged += MyGroupManager_ChampionList_NameChanged;
            }
            DisplayGroups();
        }

        private void MyGroupManager_ChampionList_NameChanged(ChampionList sender, EventArgs e) {
            DisplayGroups();
        }

        private void MySettings_ApiKeyChanged(EcsSettings sender, EventArgs e) {
            _ARR.LoadAllChampionsRiotApi();
            DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
        }

        private void btnGuide_Click(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo("https://github.com/BertGoens/EasyChampionSelection"));
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e) {
            if(_wndST != null) {
                if(_wndST.IsLoaded) {
                    _wndST.Focus();
                    return;
                }
            }

            _wndST = new wndSettings(_ARR.MySettings, _ARR.MyLolClientProcessInvokeHandler, _ARR.DisplayPopup);
            _wndST.Show();
        }

        private void btnCredits_Click(object sender, RoutedEventArgs e) {
            try {
                wndCredits wndCR = new wndCredits();
                wndCR.Owner = this;
                wndCR.ShowDialog();
            } catch(Exception) {
                _ARR.DisplayPopup("Woops, something went wrong!");
            }

        }

        private void btnNewGroup_Click(object sender, RoutedEventArgs e) {
            try {
                wndAddGroup wndAG = new wndAddGroup(_ARR.MyGroupManager);
                wndAG.Owner = this;
                wndAG.ShowDialog();
                DisplayGroups();
            } catch(Exception) {
                _ARR.DisplayPopup("Woops, something went wrong!");
            }
        }

        private void btnDeleteGroup_Click(object sender, RoutedEventArgs e) {
            if(lsbGroups.SelectedIndex > -1) {
                if(MessageBoxResult.Yes == MessageBox.Show("Remove: " + lsbGroups.SelectedItem.ToString(), "Remove group", MessageBoxButton.YesNo)) {
                    _ARR.MyGroupManager.RemoveGroup(lsbGroups.SelectedIndex);
                    lblGroupsCount.Content = "Groups: " + lsbGroups.Items.Count + " / " + _ARR.MyGroupManager.MaxGroups;
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
                _ARR.MyGroupManager.getGroup(lsbGroups.SelectedIndex).AddChampion(lbi.Content.ToString());
            }

            DisplayChampsInSelectedGroup();
            DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
        }

        private void RemoveChampionsFromCurrentGroup(object sender, RoutedEventArgs e) {
            for(int i = 0; i < lsbChampionsInGroup.SelectedItems.Count; i++) {
                ListBoxItem lbi = (ListBoxItem)lsbChampionsInGroup.SelectedItems[i];
                _ARR.MyGroupManager.getGroup(lsbGroups.SelectedIndex).RemoveChampion(lbi.Content.ToString());
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

        private void lblFilterInfo_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            lblFilterInfo.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void lblFilterInfo_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            if(txtFilterForAllChampions.Text.Length < 1) {
                lblFilterInfo.Visibility = System.Windows.Visibility.Visible;
            }
        }

        #region ContextMenus

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

            if(_ARR.MyGroupManager.MaxGroups <= _ARR.MyGroupManager.GroupCount) {
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
            wndRenameGroup wndRG = new wndRenameGroup(_ARR.MyGroupManager, lsbGroups.SelectedItem.ToString());
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
            _ARR.MyGroupManager.ReOrder((ChampionList)lsbGroups.SelectedItem, lsbGroups.SelectedIndex - 1);
            DisplayGroups();

        }

        void mniMoveGroupDown_Click(object sender, RoutedEventArgs e) {
            _ARR.MyGroupManager.ReOrder((ChampionList)lsbGroups.SelectedItem, lsbGroups.SelectedIndex + 1);
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
            _ARR.LoadAllChampionsRiotApi();
            DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
        }

        private void mniReloadLocal_Click(object sender, RoutedEventArgs e) {
            if(File.Exists(StaticSerializer.FullPath_AllChampions)) {
                _ARR.LoadAllChampionsLocal();
                DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
            } else {
                MessageBox.Show("No local saves found!", this.Title);
            }
        }

        private void mniManuallyAddChampion_Click(object sender, RoutedEventArgs e) {
            wndAddChampion wndAC = new wndAddChampion(_ARR.AllChampions);
            wndAC.Owner = this;
            wndAC.ShowDialog();
        }

        #endregion ContextMenus
        #endregion UI Events

        private MenuItem CreateMenuItem(String Header, RoutedEventHandler ClickEventHandler) {
            MenuItem mniMI = new MenuItem();
            mniMI.Header = Header;
            mniMI.Click += ClickEventHandler;
            return mniMI;
        }


        #region Public Behavior
        public void EnsureVisibility() {
            //Ensure our window is visible (I hate it when other programs splash over a user wants to open this program)
            this.Activate();
            this.Topmost = true;
            this.Topmost = false;
            this.Focus();
        }

        public void DisplayGroups() {
            int selectedItemIndex = lsbGroups.SelectedIndex;
            int groupCountBeforeOperation = lsbGroups.Items.Count;

            lsbGroups.Items.Clear();
            for(int i = 0; i < _ARR.MyGroupManager.GroupCount; i++) {
                lsbGroups.Items.Add(_ARR.MyGroupManager.getGroup(i));
            }
            lblGroupsCount.Content = "Groups: " + lsbGroups.Items.Count + " / " + _ARR.MyGroupManager.MaxGroups;
            if(_ARR.MyGroupManager.GroupCount == _ARR.MyGroupManager.MaxGroups) {
                btnNewGroup.IsEnabled = false;
            } else {
                btnNewGroup.IsEnabled = true;
            }
            if(_ARR.MyGroupManager.GroupCount == 0) {
                btnDeleteGroup.IsEnabled = false;
            } else {
                btnDeleteGroup.IsEnabled = true;
            }

            int groupsAfterOperation = lsbGroups.Items.Count;
            if(groupsAfterOperation != groupCountBeforeOperation) {
                if(selectedItemIndex + 1 > groupsAfterOperation) {
                    selectedItemIndex = -1;
                }
            }

            lsbGroups.SelectedIndex = selectedItemIndex;
        }

        public void DisplayChampsInSelectedGroup() {
            lsbChampionsInGroup.Items.Clear();
            if(lsbGroups.SelectedIndex > -1) {
                int championsInList = _ARR.MyGroupManager.getGroup(lsbGroups.SelectedIndex).ChampionCount;
                for(int i = 0; i < championsInList; i++) {
                    ListBoxItem lsbChampionsInGroupItem = new ListBoxItem();
                    lsbChampionsInGroupItem.Content = _ARR.MyGroupManager.getGroup(lsbGroups.SelectedIndex).getChampion(i);
                    lsbChampionsInGroupItem.PreviewMouseDoubleClick += lsbChampionsInGroupItem_PreviewMouseDoubleClick;
                    lsbChampionsInGroup.Items.Add(lsbChampionsInGroupItem);
                }
            }
            lblCurrentGroupChampions.Content = "Champions: " + lsbChampionsInGroup.Items.Count;
        }

        public void DisplayAllChampionsMinusInSelectedGroupAccordingToFilter() {
            lsbAllChampions.Items.Clear();
            if(lsbGroups.SelectedIndex > -1) {
                for(int i = 0; i < _ARR.AllChampions.getCount(); i++) {
                    if(!_ARR.MyGroupManager.getGroup(lsbGroups.SelectedIndex).Contains(_ARR.AllChampions.getChampion(i))) { //lsbChampionsInGroup.Items.Contains(_arr.AllChampions.getChampion(i))
                        string newChamp = _ARR.AllChampions.getChampion(i);
                        if(newChamp.ToUpper().Contains(txtFilterForAllChampions.Text.ToUpper())) {
                            ListBoxItem lsbAllChampionsItem = new ListBoxItem();
                            lsbAllChampionsItem.Content = _ARR.AllChampions.getChampion(i);
                            lsbAllChampionsItem.PreviewMouseDoubleClick += lsbAllChampionsItem_PreviewMouseDoubleClick;
                            lsbAllChampions.Items.Add(lsbAllChampionsItem);
                        }
                    }
                }
            } else {
                for(int i = 0; i < _ARR.AllChampions.getCount(); i++) {
                    string newChamp = _ARR.AllChampions.getChampion(i);
                    if(newChamp.ToUpper().Contains(txtFilterForAllChampions.Text.ToUpper())) {
                        ListBoxItem lsbAllChampionsItem = new ListBoxItem();
                        lsbAllChampionsItem.Content = _ARR.AllChampions.getChampion(i);
                        lsbAllChampionsItem.PreviewMouseDoubleClick += lsbAllChampionsItem_PreviewMouseDoubleClick;
                        lsbAllChampions.Items.Add(lsbAllChampionsItem);
                    }
                }
            }

            lblAllChampionsInfo.Content = "All champions: " + lsbAllChampions.Items.Count;
        }

        #endregion Public Behavior

    }
}