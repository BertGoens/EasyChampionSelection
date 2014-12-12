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
        wndMain wndMainBoss;

        public wndClientOverload(wndMain wndMainBoss) {
            this.wndMainBoss = wndMainBoss;
            InitializeComponent();
        }

        /// <summary>
        /// Force redraw the Client Overlay
        /// </summary>
        public void Redraw() {
            if(wndMainBoss._lolClientHelper != null) {
                if(wndMainBoss._lolClientHelper.GetProcessLolClient() != null) {
                    Rectangle pos = wndMainBoss._lolClientHelper.getClientOverlayPosition();
                    this.Left = pos.X;
                    this.Top = pos.Y;
                    //Width and height is already marked in here
                }
            }

            cboGroups.Items.Clear();
            for(int i = 0; i < wndMainBoss.gmGroupManager.GroupCount; i++) {
                System.Windows.Controls.CheckBox chk = new System.Windows.Controls.CheckBox();
                chk.Content = wndMainBoss.gmGroupManager.getGroup(i).getName();
                chk.Checked += new RoutedEventHandler(CheckBox_CheckStateChanged);
                cboGroups.Items.Add(chk);
            }
        }

        public void StaticLolClientGraphics_OnLeagueClientResized(StaticLolClientGraphics sender, EventArgs e) {
            Rectangle rect = sender.getClientOverlayPosition();
            this.Left = rect.X;
            this.Top = rect.Y;
            //Width and height are currently not being changed
        }

        public void StaticLolClientGraphics_OnLeagueClientReposition(StaticLolClientGraphics sender, EventArgs e) {
            Rectangle rect = sender.getClientOverlayPosition();
            this.Left = rect.X;
            this.Top = rect.Y;
            //Width and height are currently not being changed
        }

        public void GroupManager_GroupsChanged(StaticGroupManager sender, GroupManagerEventArgs e) {
            if(sender != null) {
                //Try to preserve checked checkboxes
                switch(e.eventOperation) {
                    case GroupManagerEventOperation.Add:
                        //Get index of new added item
                        int newItemIndex = wndMainBoss.gmGroupManager.indexOf(e.operationItem.getName());
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
                        int newPosition = wndMainBoss.gmGroupManager.indexOf(e.operationItem.getName());
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

        public void ChampionList_NameChanged(ChampionList sender, EventArgs e) {
            if(sender != null) {
                int indexOfSender = wndMainBoss.gmGroupManager.indexOf(sender.getName());

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
            String searchFieldText = CreateStringOfChampions();
            if(wndMainBoss._lolClientHelper != null) {
                if(wndMainBoss._lolClientHelper.GetProcessLolClient() != null) {
                    wndMainBoss._lolClientHelper.TypeInSearchBar(searchFieldText);
                }
            } else {
                wndMainBoss.DisplayPopup("lolClient.exe was not found!");
            }
        }

        private String CreateStringOfChampions() {
            //The client accepts regex commandos
            List<string> lstChampsToFilter = new List<string>();

            for(int i = 0; i < cboGroups.Items.Count; i++) {
                CheckBox cb = (CheckBox)cboGroups.Items[i];
                if(cb.IsChecked == true) {
                    int newGroupCount = wndMainBoss.gmGroupManager.getGroup(i).ChampionCount;
                    for(int j = 0; j < newGroupCount; j++) {
                        string newChampion = wndMainBoss.gmGroupManager.getGroup(i).getChampion(j);
                        if(!lstChampsToFilter.Contains(newChampion)) {
                            lstChampsToFilter.Add(newChampion);
                        }
                    }
                }
            }

            lstChampsToFilter.Sort();
            string returnValue = "";
            for(int i = 0; i < lstChampsToFilter.Count; i++) {
                returnValue += lstChampsToFilter[i] + "|";
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