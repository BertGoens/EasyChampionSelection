﻿using EasyChampionSelection.ECS;
using EasyChampionSelection.ECS.RiotGameData;
using EasyChampionSelection.ECS.RiotGameData.GroupManager;
using System;
using System.Windows;
using System.Windows.Controls;

namespace EasyChampionSelection {
    /// <summary>
    /// Interaction logic for wndAddGroup.xaml
    /// </summary>
    public partial class wndAddGroup : Window {
        private StaticGroupManager _gm;

        private wndAddGroup() {
            InitializeComponent();
            StaticWindowUtilities.EnsureVisibility(this);
        }
        public wndAddGroup(StaticGroupManager gm) : this() {
            if(gm != null) {
                this._gm = gm;
            } else {
                throw new ArgumentNullException();
            }
        }

        private void txtNewGroupName_TextChanged(object sender, TextChangedEventArgs e) {
            if(_gm.getAllGroups().Contains(new ChampionList(txtNewGroupName.Text))) {
                btnAdd.IsEnabled = false;
            } else {
                btnAdd.IsEnabled = true;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            if(txtNewGroupName.Text.Length > 0) {
                _gm.AddGroup(new ChampionList(txtNewGroupName.Text));
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void wndAddGroupVisual_Loaded(object sender, RoutedEventArgs e) {
            txtNewGroupName.Focus();
        }
    }
}
