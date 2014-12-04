using System.Windows;
using System.Windows.Controls;

namespace EasyChampionSelection {
    /// <summary>
    /// Interaction logic for wndRenameGroup.xaml
    /// </summary>
    public partial class wndRenameGroup : Window {
        private wndMain wndMainBoss;
        private string strGroupToRename;

        /// <summary>
        /// Constructor
        /// </summary>
        public wndRenameGroup(wndMain wndMainBoss, string strGroupToRename) {
            InitializeComponent();
            this.strGroupToRename = strGroupToRename;
            this.wndMainBoss = wndMainBoss;
        }

        private void txtRenameGroup_TextChanged(object sender, TextChangedEventArgs e) {
            if(wndMainBoss.GetGroups().Contains(txtRenameGroup.Text)) {
                btnRenameGroup.IsEnabled = false;
            } else {
                btnRenameGroup.IsEnabled = true;
            }
        }

        private void btnRenameGroup_Click(object sender, RoutedEventArgs e) {
            if(txtRenameGroup.Text.Length > 0) {
                wndMainBoss.gmGroupManager.getGroup(strGroupToRename).setName(txtRenameGroup.Text);
                wndMainBoss.DisplayGroups();
                wndMainBoss.lsbGroups.SelectedIndex = 0;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void wndRenameGroupVisual_Loaded(object sender, RoutedEventArgs e) {
            if(wndMainBoss.Equals(null) || strGroupToRename.Equals(null)) {
                MessageBox.Show("Woops, something went wrong!", "wndRenameGroup", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            lblRenameGroupNameInfo.Content = "Rename " + strGroupToRename + " to:";
            txtRenameGroup.SelectAll();
            txtRenameGroup.Focus();
        }
    }
}
