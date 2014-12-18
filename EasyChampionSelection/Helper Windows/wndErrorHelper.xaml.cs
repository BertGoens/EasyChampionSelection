using EasyChampionSelection.ECS;
using System;
using System.IO;
using System.Windows;

namespace EasyChampionSelection.Helper_Windows {
    /// <summary>
    /// Interaction logic for wndErrorHelper.xaml
    /// </summary>
    public partial class wndErrorHelper : Window {

        private Exception _error;

        public wndErrorHelper(Exception error) {
            if(error == null) {
                throw new ArgumentNullException();
            }

            _error = error;
            InitializeComponent();

            txtErrorMessage.Text = _error.ToString();
        }

        private void btnSendError_Click(object sender, RoutedEventArgs e) {
            wndContactCreator wndConCreError = new wndContactCreator(_error, txtErrorUserComment.Text);
            wndConCreError.Owner = this;
            wndConCreError.ShowDialog();
        }

        private void btnSaveError_Click(object sender, RoutedEventArgs e) {
            string dateOfToday = DateTime.Today.ToString("d");
            dateOfToday = dateOfToday.Replace("/", "_");

            string filePath = StaticSerializer.applicationPath() + StaticSerializer.PATH_Folder_ErrorData + "Error_ " + dateOfToday + ".txt";
            Directory.CreateDirectory(filePath.Substring(0, filePath.LastIndexOf(@"\")));
            using(StreamWriter sw = new StreamWriter(filePath, true)) {
                if(_error.InnerException != null) {
                    sw.WriteLine("InnerException");
                    sw.WriteLine(_error.InnerException.ToString());
                    sw.WriteLine();
                }                
                sw.WriteLine("_error.ToString()");
                sw.WriteLine(_error.ToString());
            }

            MessageBox.Show("Saved!", this.Title);
        }
    }
}
