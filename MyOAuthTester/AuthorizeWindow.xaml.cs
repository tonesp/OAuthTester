using System;
using System.Windows.Navigation;
using System.Web;

namespace MyOAuthTester
{
    /// <summary>
    /// Interaction logic for LoginModalWindow.xaml
    /// </summary>
    public partial class AuthorizeWindow
    {
        private readonly OAuthTester _oauth;
        private readonly String _tokenSecret;

        public string Token { get; private set; }

        public string Verifier { get; private set; }

        public String TokenSecret
        {
            get
            {
                return _tokenSecret;
            }
        }

        public AuthorizeWindow(OAuthTester o)
        {
            _oauth = o;
            Token = null;
            InitializeComponent();
            addressTextBox.Text = o.AuthorizationLink;
            Token = _oauth.Token;
            _tokenSecret = _oauth.TokenSecret;
            browser.Navigate(new Uri(_oauth.AuthorizationLink));            
        }

        private void BrowserNavigating(object sender, NavigatingCancelEventArgs e)
        {
            addressTextBox.Text = e.Uri.ToString();
            if (e.Uri.Scheme == "tester") {
                var queryParams = e.Uri.Query;
                if (queryParams.Length > 0)
                {
                    //Store the Token and Token Secret
                    var qs = HttpUtility.ParseQueryString(queryParams);
                    if (qs["oauth_token"] != null)
                    {
                        Token = qs["oauth_token"];
                    }
                    if (qs["oauth_verifier"] != null)
                    {
                        Verifier = qs["oauth_verifier"];
                    }
                }
                Close();
            }            
        }
    }
}
