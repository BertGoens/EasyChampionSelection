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

        private void CheckBox_CheckStateChanged(object sender, RoutedEventArgs e) {
            String searchFieldText = CreateStringOfChampions();
            if(wndMainBoss._lolClientHelper != null) {
                if(wndMainBoss._lolClientHelper.GetProcessLolClient() != null) {
                    wndMainBoss._lolClientHelper.TypeInSearchBar(searchFieldText);
                }
            } else {
                wndMainBoss.DisplayPopup(this.Title, "lolClient.exe was not found!", PopupAnimation.Fade, 4000);
            }
        }

        private String CreateStringOfChampions() {
            //The client accepts regex commandos
            List<string> lstChampsToFilter = new List<string>();
            
            for(int i = 0; i < cboGroups.Items.Count; i++) {
                CheckBox cb = (CheckBox)cboGroups.Items[i];
                if(cb.IsChecked == true) {
                    int newGroupCount = wndMainBoss.gmGroupManager.getChampionList(i).ChampionCount;
                    for(int j = 0; j < newGroupCount; j++) {
                        string newChampion = wndMainBoss.gmGroupManager.getChampionList(i).getChampion(j);
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
            returnValue = returnValue.Substring(0, returnValue.Length - 1);

            return returnValue;
        }

        private void wndMyClientOverload_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(this.IsVisible) {
                if(wndMainBoss._lolClientHelper != null) {
                    if(wndMainBoss._lolClientHelper.GetProcessLolClient() != null) {
                        Rectangle pos = wndMainBoss._lolClientHelper.GetClientOverlayPosition();
                        this.Left = pos.X;
                        this.Top = pos.Y;
                        //Width and height is already marked static
                    }
                }

                cboGroups.Items.Clear();
                for(int i = 0; i < wndMainBoss.gmGroupManager.GroupCount; i++) {
                    System.Windows.Controls.CheckBox chk = new System.Windows.Controls.CheckBox();
                    chk.Content = wndMainBoss.gmGroupManager.getChampionList(i).getName();
                    chk.Checked += new RoutedEventHandler(CheckBox_CheckStateChanged);
                    cboGroups.Items.Add(chk);
                }
            }
        }

    }
}