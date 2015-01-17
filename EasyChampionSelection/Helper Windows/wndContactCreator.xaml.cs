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

        public wndContactCreator(Action<string> DisplayMessage, Exception error, string userComment) : this(DisplayMessage) {
            txtSubject.Text = "Error ";
            if(error.InnerException != null) {
                txtSubject.Text += error.InnerException.ToString();
            }
            
            txtBody.Text = "Hello, I've encountered this error today: \n" + error.ToString();

            if(userComment.Length > 0) {
                txtBody.Text += "\n\n" + userComment;
            }
        }

        private void btnSendMail_Click(object sender, RoutedEventArgs e) {
            if(!txtFromSender.Text.Contains("@")) {
                _displayMessage("Please use a valid email address!");
                return;
            }
            if(txtFromPassword.Password.Length < 8) {
                _displayMessage("Please use a valid password!");
                return;
            }
            if(txtBody.Text.Length < 20) {
                _displayMessage("Please insert more text.");
                return;
            }

            MailAddress fromAddress = new MailAddress(txtFromSender.Text); 
            MailAddress toAddress = new MailAddress("easychampionselection@gmail.com");
            string fromPassword = txtFromPassword.Password;
            string subject = "ECS:" + txtSubject.Text;
            string body = txtBody.Text;

            try {
                SmtpClient smtp = new SmtpClient {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    Timeout = 5000,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };
                using(MailMessage message = new MailMessage(fromAddress.Address, toAddress.Address) {
                    Subject = subject,
                    Body = body
                }) {
                    smtp.Send(message);
                    _displayMessage("Thank you for you mail, I'll reach back as soon as possible!");
                }
            } catch(Exception ex) {
                _displayMessage(ex.ToString() + "\nSomething went wrong!");
            }
        }
    }
}
