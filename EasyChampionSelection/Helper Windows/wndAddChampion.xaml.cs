using EasyChampionSelection.ECS;
using EasyChampionSelection.ECS.RiotGameData;
using System;
using System.Windows;
using System.Windows.Controls;

namespace EasyChampionSelection {
    /// <summary>
    /// Interaction logic for wndAddChampion.xaml
    /// </summary>
    public partial class wndAddChampion : Window {

        private ChampionList _allChamps;

        private wndAddChampion() {
            InitializeComponent();
            StaticWindowUtilities.EnsureVisibility(this);
        }

        public wndAddChampion(ChampionList allChamps) : this() {
            if(allChamps != null) {
                this._allChamps = allChamps;
            } else {
                throw new ArgumentNullException();
            }
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
