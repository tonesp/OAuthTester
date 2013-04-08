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
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Collections.Specialized;
using System.Web;

namespace Attassa
{
    /// <summary>
    /// Interaction logic for LoginModalWindow.xaml
    /// </summary>
    public partial class AuthorizeWindow : Window
    {
        private oAuthTester _oauth;
        private String _token;
        private String _verifier;
        private String _tokenSecret;

        public String Token
        {
            get
            {
                return _token;
            }
        }

        public String Verifier
        {
            get
            {
                return _verifier;
            }
        }

        public String TokenSecret
        {
            get
            {
                return _tokenSecret;
            }
        }

        public AuthorizeWindow(oAuthTester o)
        {
            _oauth = o;
            _token = null;
            InitializeComponent();
            this.addressTextBox.Text = o.AuthorizationLink;
            _token = _oauth.Token;
            _tokenSecret = _oauth.TokenSecret;
            browser.Navigate(new Uri(_oauth.AuthorizationLink));            
        }

        private void browser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            this.addressTextBox.Text = e.Uri.ToString();
            if (e.Uri.Scheme == "tester") {
                string queryParams = e.Uri.Query;
                if (queryParams.Length > 0)
                {
                    //Store the Token and Token Secret
                    NameValueCollection qs = HttpUtility.ParseQueryString(queryParams);
                    if (qs["oauth_token"] != null)
                    {
                        _token = qs["oauth_token"];
                    }
                    if (qs["oauth_verifier"] != null)
                    {
                        _verifier = qs["oauth_verifier"];
                    }
                }
                this.Close();
            }            
        }
    }
}
