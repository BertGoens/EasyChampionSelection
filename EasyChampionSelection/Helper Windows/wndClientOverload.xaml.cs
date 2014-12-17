using EasyChampionSelection.ECS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace EasyChampionSelection {
    /// <summary>
    /// Interaction logic for wndClientOverload.xaml
    /// </summary>
    public partial class wndClientOverload : Window {

        private StaticGroupManager _groupManager;
        private StaticLolClientGraphics _lcg;

        public wndClientOverload(StaticGroupManager gmGroupManager, StaticLolClientGraphics lcg) {
            if(gmGroupManager != null && lcg != null) {
                this._groupManager = gmGroupManager;
                this._lcg = lcg;
                _groupManager.GroupsChanged += _groupManager_GroupsChanged;
                _lcg.OnLeagueClientReposition += _lcg_OnLeagueClientReposition;
                for(int i = 0; i < _groupManager.GroupCount; i++) {
                    _groupManager.getGroup(i).NameChanged += _groupManager_ChampionList_NameChanged;
                }
            } else {
                MessageBox.Show("wndClientOverload has null parameters!");
            }

            InitializeComponent();
            Redraw();
        }

        public void SafeDelete() {
            _groupManager.GroupsChanged -= _groupManager_GroupsChanged;
            _lcg.OnLeagueClientReposition -= _lcg_OnLeagueClientReposition;
            for(int i = 0; i < _groupManager.GroupCount; i++) {
                _groupManager.getGroup(i).NameChanged -= _groupManager_ChampionList_NameChanged;
            }
            Close();
        }

        /// <summary>
        /// Force redraw the Client Overlay
        /// </summary>
        private void Redraw() {
            if(_lcg.getProcessLolClient() != null) {
                Rectangle pos = _lcg.getClientOverlayPosition();
                this.Left = pos.X;
                this.Top = pos.Y;
                cboGroups.Width = Math.Abs(pos.Width - cboGroups.Margin.Left - 5);
                this.Width = pos.Width;
                this.Height = pos.Height;
            }

            cboGroups.Items.Clear();
            for(int i = 0; i < _groupManager.GroupCount; i++) {
                System.Windows.Controls.CheckBox chk = new System.Windows.Controls.CheckBox();
                chk.Content = _groupManager.getGroup(i).getName();
                chk.Checked += new RoutedEventHandler(CheckBox_CheckStateChanged);
                cboGroups.Items.Add(chk);
            }
        }

        public void RepositionClientOverlay() {
            Rectangle rect = _lcg.getClientOverlayPosition();
            this.Left = rect.X;
            this.Top = rect.Y;
            this.Width = rect.Width;
            this.Height = rect.Height;
        }

        private void _lcg_OnLeagueClientReposition(StaticLolClientGraphics sender, EventArgs e) {
            Rectangle rect = sender.getClientOverlayPosition();
            this.Left = rect.X;
            this.Top = rect.Y;
        }

        private void _groupManager_GroupsChanged(StaticGroupManager sender, GroupManagerEventArgs e) {
            if(sender != null) {
                //Try to preserve checked checkboxes
                switch(e.eventOperation) {
                    case GroupManagerEventOperation.Add:
                        //Get index of new added item
                        int newItemIndex = _groupManager.indexOf(e.operationItem.getName());
                        //Insert item here at the correct spot
                        System.Windows.Controls.CheckBox chk = new System.Windows.Controls.CheckBox();
                        chk.Content = e.operationItem.getName();
                        chk.Checked += new RoutedEventHandler(CheckBox_CheckStateChanged);
                        cboGroups.Items.Insert(newItemIndex, chk);
                        break;

                    case GroupManagerEventOperation.Remove:
                        //Remove it here based on e known name
                        for(int i = 0; i < cboGroups.Items.Count; i++) {
                            CheckBox cb = (CheckBox)cboGroups.Items[i];
                            if(cb.Content.ToString() == e.operationItem.getName()) {
                                cboGroups.Items.RemoveAt(i);
                                break;
                            }
                        }
                        break;

                    case GroupManagerEventOperation.Reposition:
                        //Reposition it here based on name
                        //Get new position
                        int newPosition = _groupManager.indexOf(e.operationItem.getName());
                        int oldPosition = -1;
                        bool isChecked = false;
                        //Find old position
                        for(int i = 0; i < cboGroups.Items.Count; i++) {
                            CheckBox cb = (CheckBox)cboGroups.Items[i];
                            if(cb.Content.ToString() == e.operationItem.getName()) {
                                oldPosition = i;
                                isChecked = cb.IsChecked.Value;
                                cboGroups.Items.RemoveAt(i);
                                break;
                            }
                        }

                        //Make new checkbox at that index
                        System.Windows.Controls.CheckBox chkReplace = new System.Windows.Controls.CheckBox();
                        chkReplace.Content = e.operationItem.getName();
                        chkReplace.IsChecked = isChecked;
                        chkReplace.Checked += new RoutedEventHandler(CheckBox_CheckStateChanged);
                        cboGroups.Items.Insert(newPosition, chkReplace);
                        break;
                }

                cboGroups.UpdateLayout();
            }
        }

        private void _groupManager_ChampionList_NameChanged(ChampionList sender, EventArgs e) {
            if(sender != null) {
                int indexOfSender = _groupManager.indexOf(sender.getName());

                //if indexOfSender != -1 update that specific group
                if(indexOfSender != -1) {
                    System.Windows.Controls.CheckBox chk = new System.Windows.Controls.CheckBox();
                    chk = (System.Windows.Controls.CheckBox)cboGroups.Items.GetItemAt(indexOfSender);
                    chk.Content = sender.getName();
                    cboGroups.UpdateLayout();
                }
            }
        }

        private void CheckBox_CheckStateChanged(object sender, RoutedEventArgs e) {
            this._lcg.TypeInSearchBar(CreateStringOfCheckedItems());
        }

        private void cboGroups_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            _lcg.TypeInSearchBar(CreateStringOfChampionsInChampionList(_groupManager.getGroup(cboGroups.SelectedIndex)));
        }

        private string CreateStringOfChampionsInChampionList(ChampionList c) {
            //The client accepts regex commandos

            string returnValue = "";
            for(int i = 0; i < c.getCount(); i++) {
                returnValue += c.getChampion(i) + "|";
            }

            //replace whitespace with \s (regex space)
            returnValue = returnValue.Replace(" ", "\\s");

            //remove last |
            if(returnValue.Length > 0) {
                returnValue = returnValue.Substring(0, returnValue.Length - 1);
            }

            return returnValue;
        }

        private string CreateStringOfCheckedItems() {
            string returnValue = "";

            List<string> lstChampsToFilter = new List<string>();

            for(int i = 0; i < cboGroups.Items.Count; i++) {
                CheckBox cb = (CheckBox)cboGroups.Items[i];
                if(cb.IsChecked == true) {
                    int newGroupCount = _groupManager.getGroup(i).ChampionCount;
                    for(int j = 0; j < newGroupCount; j++) {
                        string newChampion = _groupManager.getGroup(i).getChampion(j);
                        if(!lstChampsToFilter.Contains(newChampion)) {
                            lstChampsToFilter.Add(newChampion);
                            returnValue += newChampion + "|";
                        }
                    }
                }
            }
            //replace whitespace with \s (regex space)
            returnValue = returnValue.Replace(" ", "\\s");

            //remove last |
            if(returnValue.Length > 0) {
                returnValue = returnValue.Substring(0, returnValue.Length - 1);
            }

            return returnValue;
        }

    }
}