using System;
using System.Net;
using System.Net.Mail;
using System.Windows;

namespace EasyChampionSelection.Helper_Windows {
    /// <summary>
    /// Interaction logic for wndContactCreator.xaml
    /// </summary>
    public partial class wndContactCreator : Window {
        public wndContactCreator() {
            InitializeComponent();
        }

        public wndContactCreator(Exception error, string userComment) : this() {
            txtSubject.Text = "Error " + error.InnerException.ToString();
            txtBody.Text = "Hello, I've encountered this error today: \n" +
                error.ToString();

            if(userComment.Length > 0) {
                txtBody.Text += "\n\n" + userComment;
            }
        }

        private void btnSendMail_Click(object sender, RoutedEventArgs e) {
            if(!txtFromSender.Text.Contains("@")) {
                MessageBox.Show("Please use a valid email address!");
                return;
            }
            if(txtFromPassword.Password.Length < 8) {
                MessageBox.Show("Please use a valid password!");
                return;
            }
            if(txtBody.Text.Length < 20) {
                MessageBox.Show("Please insert more text.");
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
                    MessageBox.Show("Thank you for you mail, I'll reach back as soon as possible!", this.Title);
                }
            } catch(SmtpException) {
                MessageBox.Show("Are you using verification codes to access your account? "
                    + "\nIf so please send your mail trough your browser.","Something went wrong!");
            } catch(Exception ex) {
                MessageBox.Show(ex.ToString(),"Something went wrong!");
            }
        }
    }
}
