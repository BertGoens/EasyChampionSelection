using EasyChampionSelection.ECS;
using System;
using System.IO;
using System.Windows;

namespace EasyChampionSelection.Helper_Windows {
    /// <summary>
    /// Interaction logic for wndErrorHelper.xaml
    /// </summary>
    public partial class wndErrorHelper : Window {

        private Action<string, bool, Window> _displayPopup;
        private Exception _error;

        private wndErrorHelper() {
            InitializeComponent();
        }

        public wndErrorHelper(Exception error, Action<string, bool, Window> DisplayPopup) : this() {
            if(error == null) {
                throw new ArgumentNullException();
            }

            _error = error;
            _displayPopup = DisplayPopup;

            txtErrorMessage.Text = _error.ToString();
        }

        private void btnSendError_Click(object sender, RoutedEventArgs e) {
            wndContactCreator wndConCreError = new wndContactCreator(_error, txtErrorUserComment.Text);
            wndConCreError.Owner = this;
            wndConCreError.ShowDialog();
        }

        private void btnSaveError_Click(object sender, RoutedEventArgs e) {
            using(StreamWriter sw = new StreamWriter(StaticSerializer.FullPath_ErrorFile, true)) {
                if(_error.InnerException != null) {
                    sw.WriteLine("InnerException");
                    sw.WriteLine(_error.InnerException.ToString());
                    sw.WriteLine();
                }                
                sw.WriteLine("_error.ToString()");
                sw.WriteLine(_error.ToString());
                sw.WriteLine();
            }

            _displayPopup("Saved!", false, this);
        }
    }
}
