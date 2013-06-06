using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyOAuthTester
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        private readonly OAuthTester _oauth = new OAuthTester();
        private bool _firstTime = true;

        private static IEnumerable<string> LoadDataPlatforms()
        {
            string[] strArray ={
                "http://apifree.ntrglobal.com",
                "http://apieu.ntrglobal.com",
                "http://212.0.111.116"
                };
            return strArray;
        }
        private static IEnumerable<string> LoadDataMethods()
        {
            string[] strArray ={
                "GET",
                "POST",
                "PUT"
                };
            return strArray;
        }

        public Window1()
        {
            InitializeComponent();
            cboPlatform.ItemsSource = LoadDataPlatforms();
            cboMethod.ItemsSource = LoadDataMethods();
        }

        private static bool IsEmpty(string myObject)
        {
            if (myObject == "")
            {
                return true;
            }
            return false;
        }


        private bool CheckValidOauthParams()
        {
            if (IsEmpty(txtKey.Text)) return false;
            if (IsEmpty(txtSecret.Text)) return false;
            if (IsEmpty(txtPlatform.Text)) return false;
            if (IsEmpty(txtOAuthToken.Text)) return false;
            if (IsEmpty(txtOAuthTokenSecret.Text)) return false;
            if (IsEmpty(txtOAuthVerifier.Text)) return false;
            return true;
        }

        private bool InitializeOauthValues()
        {
            if (CheckValidOauthParams())
            {
                _oauth.ConsumerKey = txtKey.Text;
                _oauth.ConsumerSecret = txtSecret.Text;
                _oauth.Platform = txtPlatform.Text;
                _oauth.Token = txtOAuthToken.Text;
                _oauth.TokenSecret = txtOAuthTokenSecret.Text;
                _oauth.Verifier = txtOAuthVerifier.Text;
                _oauth.Platform = txtPlatform.Text;
                return true;
            }
            txtOutput.Text = "\nPlease, fill your oAuth Params";
            return false;
        }

        

        private void LoginClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!InitializeOauthValues()) return;

                var requestToken = _oauth.GetRequestToken();
                txtOutput.Text += "\n" + "Received request token: " + requestToken;
                _oauth.AuthorizeToken();
                txtOutput.Text += "\n" + "Token was authorized: " + _oauth.Token + " with verifier: " + _oauth.Verifier;
                //var accessToken = _oauth.getAccessToken();
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

        private void TxtPlatform_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (_firstTime)
            {
                txtPlatform.Text = "";
                txtPlatform.Foreground = Brushes.Black;
            }
            _firstTime = false;
        }
        private void TxtPlatform_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (!Uri.IsWellFormedUriString(txtPlatform.Text, UriKind.Absolute))
                txtPlatform.Text = "http://apifree.ntrglobal.com";
        }

        private void CboPlatform_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtPlatform.Text = cboPlatform.SelectedValue.ToString();
            txtPlatform.Foreground = Brushes.Black;
            lblPlatform.Content = cboPlatform.SelectedValue.ToString();
            _firstTime = false;

        }

        private void DoAction(object sender, RoutedEventArgs e)
        {
            if (!InitializeOauthValues()) return;
            if (cboMethod.SelectedIndex == -1) return;
            var sMethod = cboMethod.SelectedValue.ToString();
            var sUrl = _oauth.Platform + txtURLPOST.Text;
            try
            {
                switch (sMethod)
                {
                    case "GET":
                        txtOutput.Text = sUrl;
                        txtOutput.Text += "\n" + _oauth.ApiWebRequest("GET", sUrl, null);
                        break;
                    default: //"POST", "PUT":
                        txtOutput.Text = sUrl;
                        txtOutput.Text += "\n" + _oauth.ApiWebRequest(sMethod, sUrl, txtBodyPOST.ToString());
                        break;
                }
            }
            catch (Exception exp)
            {
                txtOutput.Text = sUrl;
                txtOutput.Text += "\nException: " + exp.Message;
            }
        }
    }
}
