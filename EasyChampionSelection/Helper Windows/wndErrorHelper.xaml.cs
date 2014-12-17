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
            if(error != null) {
                _error = error;
            }
            InitializeComponent();

            txtErrorMessage.Text = _error.ToString();
        }

        private void btnSendError_Click(object sender, RoutedEventArgs e) {
            wndContactCreator wndConCreError = new wndContactCreator(_error, txtErrorUserComment.Text);
            wndConCreError.Owner = this.Owner;
            wndConCreError.Show();
        }

        private async void btnSaveError_Click(object sender, RoutedEventArgs e) {
            string dateOfToday = DateTime.Today.ToString("d");
            dateOfToday = dateOfToday.Replace("/", "_");

            using(StreamWriter sw = new StreamWriter("Error_ " + dateOfToday + ".txt")) {
                await sw.WriteLineAsync("InnerException");
                await sw.WriteLineAsync(_error.InnerException.ToString());
                await sw.WriteLineAsync();
                await sw.WriteLineAsync("_error.ToString()");
                await sw.WriteLineAsync(_error.ToString());
            }
        }
    }
}
