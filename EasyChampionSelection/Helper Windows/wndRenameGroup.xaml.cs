using EasyChampionSelection.ECS.RiotGameData;
using EasyChampionSelection.ECS.RiotGameData.GroupManager;
using System;
using System.Windows;
using System.Windows.Controls;

namespace EasyChampionSelection {
    /// <summary>
    /// Interaction logic for wndRenameGroup.xaml
    /// </summary>
    public partial class wndRenameGroup : Window {
        private StaticGroupManager _gm;
        private string _strGroupToRename;

        /// <summary>
        /// Constructor
        /// </summary>
        public wndRenameGroup(StaticGroupManager gm, string strGroupToRename) {
            if(gm != null && strGroupToRename != null) {
                this._gm = gm;
                this._strGroupToRename = strGroupToRename;
            } else {
                throw new ArgumentNullException();
            }
            InitializeComponent();
        }

        private void txtRenameGroup_TextChanged(object sender, TextChangedEventArgs e) {
            if(_gm.getAllGroups().Contains(new ChampionList(txtRenameGroup.Text))) {
                btnRenameGroup.IsEnabled = false;
            } else {
                btnRenameGroup.IsEnabled = true;
            }
        }

        private void btnRenameGroup_Click(object sender, RoutedEventArgs e) {
            if(txtRenameGroup.Text.Length > 0) {
                _gm.getGroup(_strGroupToRename).setName(txtRenameGroup.Text);
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void wndRenameGroupVisual_Loaded(object sender, RoutedEventArgs e) {
            lblRenameGroupNameInfo.Content = "Rename " + _strGroupToRename + " to:";
            txtRenameGroup.SelectAll();
            txtRenameGroup.Focus();
        }
    }
}
