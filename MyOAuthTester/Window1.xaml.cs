using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
namespace Attassa
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private oAuthTester _oauth = new oAuthTester();

        public Window1()
        {
            InitializeComponent();
        }

        private void login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _oauth.ConsumerKey = txtKey.Text;
                _oauth.ConsumerSecret = txtSecret.Text;
                _oauth.Platform = cboPlatform.Text;

                String requestToken = _oauth.getRequestToken();
                txtOutput.Text += "\n" + "Received request token: " + requestToken;
                _oauth.authorizeToken();
                txtOutput.Text += "\n" + "Token was authorized: " + _oauth.Token + " with verifier: " + _oauth.Verifier;
                String accessToken = _oauth.getAccessToken();
                txtOutput.Text += "\n" + "Access token was received: " + _oauth.Token;

                txtOAuthToken.Text = _oauth.Token;
                txtOAuthTokenSecret.Text = _oauth.TokenSecret;
                txtOAuthVerifier.Text = _oauth.Verifier;

                btnLogin.IsEnabled = false;
            }
            catch (Exception exp)
            {
                txtOutput.Text = "\nException: " + exp.Message; 
            }
        }
        private void GetDevices_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _oauth.ConsumerKey = txtKey.Text;
                _oauth.ConsumerSecret = txtSecret.Text;
                _oauth.Platform = cboPlatform.Text;
                _oauth.Token = txtOAuthToken.Text;
                _oauth.TokenSecret = txtOAuthTokenSecret.Text;
                _oauth.Verifier = txtOAuthVerifier.Text;
                txtOutput.Text = "\n" + _oauth.APIWebRequest("GET", cboPlatform.Text +"/devices.xml" , null);
            }
            catch (Exception exp)
            {
                txtOutput.Text = "\nException: " + exp.Message;
            }

        }
        private void GetUsers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtOutput.Text = "\n" + _oauth.APIWebRequest("GET", cboPlatform.Text + "/users.xml", null);
            }
            catch (Exception exp)
            {
                txtOutput.Text = "\nException: " + exp.Message;
            }

        }
        private void GetSession_Click(object sender, RoutedEventArgs e)
        {
            string sPost ="";
            try
            {
                sPost = "<support_session><customer>CustomerName</customer><customer_mail>email@domain.com</customer_mail><language>en</language></support_session>";
                txtOutput.Text = "\n" + _oauth.APIWebRequest("POST", cboPlatform.Text +"/support_sessions.xml", sPost);
            }
            catch (Exception exp)
            {
                txtOutput.Text = "\nException: " + exp.Message;
            }
        }
    }
}
