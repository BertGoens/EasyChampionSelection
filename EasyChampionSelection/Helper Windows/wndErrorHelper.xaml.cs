using EasyChampionSelection.ECS;
using System;
using System.IO;
using System.Windows;

namespace EasyChampionSelection.Helper_Windows {
    /// <summary>
    /// Interaction logic for wndErrorHelper.xaml
    /// </summary>
    public partial class wndErrorHelper : Window {

        private Action<string> _displayPopup;
        private Exception _error;
        private wndContactCreator _wndConCreError;

        private wndErrorHelper() {
            InitializeComponent();
            StaticWindowUtilities.EnsureVisibility(this);
        }

        public wndErrorHelper(Exception error, Action<string> DisplayPopup) : this() {
            if(error == null) {
                throw new ArgumentNullException();
            }

            StaticErrorLogger.WriteErrorReport(_error, "Handled");

            _error = error;
            _displayPopup = DisplayPopup;

            txtErrorMessage.Text = _error.ToString();
        }

        private void btnSendError_Click(object sender, RoutedEventArgs e) {
            if(_wndConCreError == null) {
                _wndConCreError = new wndContactCreator(_displayPopup, _error);
                _wndConCreError.Closed += (s, args) => _wndConCreError = null;
            } else {
                StaticWindowUtilities.EnsureVisibility(this);
            }
            _wndConCreError.Show();
        }
    }
}
