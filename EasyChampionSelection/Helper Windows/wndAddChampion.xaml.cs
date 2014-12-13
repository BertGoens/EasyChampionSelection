using EasyChampionSelection.ECS;
using System;
using System.Windows;
using System.Windows.Controls;

namespace EasyChampionSelection {
    /// <summary>
    /// Interaction logic for wndAddChampion.xaml
    /// </summary>
    public partial class wndAddChampion : Window {

        private ChampionList _allChamps;

        public wndAddChampion(ChampionList allChamps) {
            if(allChamps != null) {
                this._allChamps = allChamps;
            } else {
                throw new ArgumentNullException();
            }
            
            InitializeComponent();
        }

        private void txtNewChampion_TextChanged(object sender, TextChangedEventArgs e) {
            if(_allChamps.Contains(txtNewChampionName.Text)) {
                btnAddChampion.IsEnabled = false;
            } else {
                btnAddChampion.IsEnabled = true;
            }
        }

        private void btnAddChampion_Click(object sender, RoutedEventArgs e) {
            if(txtNewChampionName.Text.Length > 0) {
                _allChamps.AddChampion(txtNewChampionName.Text);
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void wndAddChampion_Loaded(object sender, RoutedEventArgs e) {
            txtNewChampionName.Focus();
        }
    }
}
