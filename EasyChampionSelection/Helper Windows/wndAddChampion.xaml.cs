using System.Windows;
using System.Windows.Controls;

namespace EasyChampionSelection {
    /// <summary>
    /// Interaction logic for wndAddChampion.xaml
    /// </summary>
    public partial class wndAddChampion : Window {

        private wndMain wndMainBoss;

        public wndAddChampion(wndMain wndMainBoss) {
            this.wndMainBoss = wndMainBoss;
            InitializeComponent();
        }

        private void txtNewChampion_TextChanged(object sender, TextChangedEventArgs e) {
            if(wndMainBoss.GetAllChampions().Contains(txtNewChampionName.Text)) {
                btnAddChampion.IsEnabled = false;
            } else {
                btnAddChampion.IsEnabled = true;
            }
        }

        private void btnAddChampion_Click(object sender, RoutedEventArgs e) {
            if(txtNewChampionName.Text.Length > 0) {
                wndMainBoss.AddChampion(txtNewChampionName.Text);
                wndMainBoss.DisplayAllChampionsMinusInSelectedGroupAccordingToFilter();
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void wndAddChampion_Loaded(object sender, RoutedEventArgs e) {
            if(wndMainBoss.Equals(null)) {
                MessageBox.Show("Woops, something went wrong!", this.Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
