using EasyChampionSelection.ECS;
using System;
using System.Net;
using System.Net.Mail;
using System.Windows;

namespace EasyChampionSelection.Helper_Windows {
    /// <summary>
    /// Interaction logic for wndContactCreator.xaml
    /// </summary>
    public partial class wndContactCreator : Window {

        private Action<string> _displayMessage;

        private wndContactCreator() {
            InitializeComponent();
            StaticWindowUtilities.EnsureVisibility(this);
        }

        public wndContactCreator(Action<string> DisplayMessage) : this() {
            _displayMessage = DisplayMessage;
        }

        public wndContactCreator(Action<string> DisplayMessage, Exception error) : this(DisplayMessage) {
            txtSubject.Text = "Error ";

            txtBody.Text = "Hello, I've encountered this error today: \n" + StaticErrorLogger.FormalizeError(error, "Handled");
        }

        private void btnSendMail_Click(object sender, RoutedEventArgs e) {
            if(!txtFromSender.Text.Contains("@")) {
                _displayMessage("Please use a valid email address!");
                return;
            }

            if(txtBody.Text.Length < 20) {
                _displayMessage("Please insert more text.");
                return;
            }

            string to = "EasyChampionSelection@gmail.com";
            string from = txtFromSender.Text;
            string subject = txtSubject.Text;
            string body = txtBody.Text;
            MailMessage message = new MailMessage(from, to, subject, body);
            SmtpClient client = new SmtpClient {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                Timeout = 5000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
            };
            Console.WriteLine("Changing time out from {0} to 100.", client.Timeout);
            client.Timeout = 100;
            // Credentials are necessary if the server requires the client 
            // to authenticate before it will send e-mail on the client's behalf.
            client.Credentials = CredentialCache.DefaultNetworkCredentials;

            try {
                client.Send(message);
            } catch(Exception ex) {
                _displayMessage("Problem encountered while sending mail, please try again!\n" + ex.Message);
            }
        }
    }
}
