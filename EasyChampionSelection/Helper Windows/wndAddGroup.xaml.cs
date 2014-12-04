using System.Windows;
using System.Windows.Controls;

namespace EasyChampionSelection {
    /// <summary>
    /// Interaction logic for wndAddGroup.xaml
    /// </summary>
    public partial class wndAddGroup : Window {
        private wndMain wndMainBoss;

        /// <summary>
        /// Constructor
        /// </summary>
        public wndAddGroup(wndMain wndMainBoss) {
            this.wndMainBoss = wndMainBoss;
            InitializeComponent();
        }

        private void txtNewGroupName_TextChanged(object sender, TextChangedEventArgs e) {
            if(wndMainBoss.GetGroups().Contains(txtNewGroupName.Text)) {
                btnAdd.IsEnabled = false;
            } else {
                btnAdd.IsEnabled = true;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            if(txtNewGroupName.Text.Length > 0) {
                wndMainBoss.AddGroup(txtNewGroupName.Text);
                wndMainBoss.DisplayGroups();
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void wndAddGroupVisual_Loaded(object sender, RoutedEventArgs e) {
            if(wndMainBoss.Equals(null)) {
                MessageBox.Show("Woops, something went wrong!", this.Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            txtNewGroupName.Focus();
        }
    }
}
