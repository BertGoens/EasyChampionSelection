using EasyChampionSelection.ECS;
using System;
using System.IO;
using System.Windows;

namespace EasyChampionSelection.Helper_Windows {
    /// <summary>
    /// Interaction logic for wndErrorHelper.xaml
    /// </summary>
    public partial class wndErrorHelper : Window {

        private StaticTaskbarManager _MyTaskbarManager;
        private Exception _error;

        private wndErrorHelper() {
            InitializeComponent();
        }

        public wndErrorHelper(Exception error, StaticTaskbarManager tbm) : this() {
            if(error == null) {
                throw new ArgumentNullException();
            }

            _error = error;
            _MyTaskbarManager = tbm;

            txtErrorMessage.Text = _error.ToString();
        }

        private void btnSendError_Click(object sender, RoutedEventArgs e) {
            wndContactCreator wndConCreError = new wndContactCreator(_error, txtErrorUserComment.Text);
            wndConCreError.Owner = this;
            wndConCreError.ShowDialog();
        }

        private void btnSaveError_Click(object sender, RoutedEventArgs e) {
            Directory.CreateDirectory(StaticSerializer.FullPath_ErrorFile.Substring(0, StaticSerializer.FullPath_ErrorFile.LastIndexOf(@"\")));
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

            _MyTaskbarManager.DisplayPopup("Saved!", false, this);
        }
    }
}
