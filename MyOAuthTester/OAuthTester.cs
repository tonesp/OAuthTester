/*
 * Example from http://davidquail.com/2009/12/17/linkedin-oauth-desktop-c-sample-embedded-webbrowser/
 * Originally used for LinkdIn comunications, but customized for our purposes
 * 
*/

using System;
using System.Web;
using System.Net;
using System.IO;
using System.Text;

namespace MyOAuthTester
{

    public class OAuthTester : OAuthBase
    {
        /*Consumer settings*/
        private string _consumerKey = "";
        private string _consumerSecret = "";
        private string _platform = "";
        private string _token = "";
        private string _tokenSecret = "";
        private string _accessToken = "";

        #region PublicProperties
        public string Platform { get { return _platform; } set { _platform = value; } }
        public string ConsumerKey { get { return _consumerKey; } set { _consumerKey = value; } }
        public string ConsumerSecret { get { return _consumerSecret; } set { _consumerSecret = value; } }
        public string Token { get { return _token; } set { _token = value; } }
        public string TokenSecret { get { return _tokenSecret; } set { _tokenSecret = value; } }
        public string AccessToken { get { return _accessToken; } set { _accessToken = value; } }
        #endregion

        public enum Method { GET, POST, PUT, DELETE };
        public const string UserAgent = "TestFREE";
        public const string RequestTokenUrl = "/oauth/request_token";
        public const string AuthorizeUrl = "/oauth/authorize";
        public const string AccessTokenUrl = "/oauth/access_token";
        public const string Callback = "tester://success";
        //public const string CALLBACK = "";

        

        /// <summary>
        /// Get the linkedin request token using the consumer key and secret.  Also initializes tokensecret
        /// </summary>
        /// <returns>The request token.</returns>
        public String GetRequestToken() {
            string ret = null;
            var response = OAuthWebRequest(Method.POST, Platform + RequestTokenUrl, String.Empty);
            if (response.Length > 0)
            {
                var qs = HttpUtility.ParseQueryString(response);
                if (qs["oauth_token"] != null)
                {
                    Token = qs["oauth_token"];
                    TokenSecret = qs["oauth_token_secret"];
                    ret = Token;
                }
            }
            return ret;        
        }

        /// <summary>
        /// Authorize the token by showing the dialog
        /// </summary>
        /// <returns>The request token.</returns>
        public String AuthorizeToken() {
            if (string.IsNullOrEmpty(Token))
            {
                var e = new Exception("The request token is not set");
                throw e;
            }

            var aw = new AuthorizeWindow(this);
            aw.ShowDialog();
            Token = aw.Token;
            Verifier = aw.Verifier;
            if (!string.IsNullOrEmpty(Verifier))
            {
                return Token;
            }
            return null;
        }

        /// <summary>
        /// Get the access token
        /// </summary>
        /// <returns>The access token.</returns>        
        public String GetAccessToken() {
            if (string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(Verifier))
            {
                var e = new Exception("The request token and verifier were not set");
                throw e;
            }

            string response = OAuthWebRequest(Method.POST, Platform + AccessTokenUrl, string.Empty);

            if (response.Length > 0)
            {
                var qs = HttpUtility.ParseQueryString(response);
                if (qs["oauth_token"] != null)
                {
                    Token = qs["oauth_token"];
                    _token = Token;
                }
                if (qs["oauth_token_secret"] != null)
                {
                    TokenSecret = qs["oauth_token_secret"];
                    _tokenSecret = TokenSecret;
                }
            }

            return Token;        
        }

        /// <summary>
        /// Get the link to Linked In's authorization page for this application.
        /// </summary>
        /// <returns>The url with a valid request token, or a null string.</returns>
        public string AuthorizationLink
        {
            get { return Platform + AuthorizeUrl + "?oauth_token=" + Token; }
        }

        /// <summary>
        /// Submit a web request using oAuth.
        /// </summary>
        /// <param name="method">GET or POST</param>
        /// <param name="url">The full url, including the querystring.</param>
        /// <param name="postData">Data to post (querystring format)</param>
        /// <returns>The web server response.</returns>
        public string OAuthWebRequest(Method method, string url, string postData)
        {
            string outUrl;
            string querystring;
            var ret = "";

            //Setup postData for signing.
            //Add the postData to the querystring.
            if (method == Method.POST)
            {
                if (postData.Length > 0)
                {
                    //Decode the parameters and re-encode using the oAuth UrlEncode method.
                    var qs = HttpUtility.ParseQueryString(postData);
                    postData = "";
                    foreach (var key in qs.AllKeys)
                    {
                        if (postData.Length > 0)
                        {
                            postData += "&";
                        }
                        qs[key] = HttpUtility.UrlDecode(qs[key]);
                        qs[key] = UrlEncode(qs[key]);
                        postData += key + "=" + qs[key];

                    }
                    if (url.IndexOf("?", StringComparison.Ordinal) > 0)
                    {
                        url += "&";
                    }
                    else
                    {
                        url += "?";
                    }
                    url += postData;
                }
            }

            var uri = new Uri(url);

            string nonce = GenerateNonce();
            string timeStamp = GenerateTimeStamp();
            
            string callback = "";
            if (url.Contains(Platform + RequestTokenUrl))
                callback = Callback;

            //Generate Signature
            string sig = GenerateSignature(uri,
                ConsumerKey,
                ConsumerSecret,
                Token,
                TokenSecret,
                method.ToString(),
                timeStamp,
                nonce,
                callback,
                out outUrl,
                out querystring);


            querystring += "&oauth_signature=" + HttpUtility.UrlEncode(sig);

            //Convert the querystring to postData
            if (method == Method.POST)
            {
                postData = querystring;
                querystring = "";
            }

            if (querystring.Length > 0)
            {
                outUrl += "?";
            }

            if (method == Method.POST || method == Method.GET)
                ret = WebRequest(method, outUrl + querystring, postData);
                
            return ret;
        }

        /// <summary>
        /// WebRequestWithPut
        /// </summary>
        /// <param name="method">WebRequestWithPut</param>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        public string ApiWebRequest(string method, string url, string postData)
        {
            var uri = new Uri(url);
            var nonce = GenerateNonce();
            var timeStamp = GenerateTimeStamp();

            string outUrl, querystring;

            //Generate Signature
            var sig = GenerateSignature(uri,
                ConsumerKey,
                ConsumerSecret,
                Token,
                TokenSecret,
                method,
                timeStamp,
                nonce,
                null,
                out outUrl,
                out querystring);

            var webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;
            if (webRequest != null)
            {
                webRequest.Method = method;
                webRequest.Credentials = CredentialCache.DefaultCredentials;
                webRequest.AllowWriteStreamBuffering = true;
                webRequest.ContentType = "text/xml";
                webRequest.PreAuthenticate = true;
                webRequest.ServicePoint.Expect100Continue = false;
            }
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;

            string responseData = null;
            if (webRequest != null)
            {
                webRequest.Headers.Add("Authorization", "OAuth realm=\"" + Platform + "\",oauth_consumer_key=\"" + ConsumerKey + "\",oauth_token=\"" + Token + "\",oauth_signature_method=\"HMAC-SHA1\",oauth_signature=\"" + HttpUtility.UrlEncode(sig) + "\",oauth_timestamp=\"" + timeStamp + "\",oauth_nonce=\"" + nonce + "\",oauth_verifier=\"" + Verifier + "\", oauth_version=\"1.0\"");            

                if (postData != null)
                {
                    var fileToSend = Encoding.UTF8.GetBytes(postData);
                    webRequest.ContentLength = fileToSend.Length;

                    var reqStream = webRequest.GetRequestStream();

                    reqStream.Write(fileToSend, 0, fileToSend.Length);
                    reqStream.Close();
                }

                responseData = WebResponseGet(webRequest);
            }

            return responseData;
        }


        /// <summary>
        /// Web Request Wrapper
        /// </summary>
        /// <param name="method">Http Method</param>
        /// <param name="url">Full url to the web resource</param>
        /// <param name="postData">Data to post in querystring format</param>
        /// <returns>The web server response.</returns>
        public string WebRequest(Method method, string url, string postData)
        {
            string responseData = "";

            var webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;
            if (webRequest != null)
            {
                webRequest.Method = method.ToString();
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.UserAgent  = UserAgent;
                webRequest.Timeout = 20000;

                if (method == Method.POST)
                {
                    webRequest.ContentType = "application/x-www-form-urlencoded";

                    var requestWriter = new StreamWriter(webRequest.GetRequestStream());
                    try
                    {
                        requestWriter.Write(postData);
                    }
                    finally
                    {
                        requestWriter.Close();
                    }
                }

                responseData = WebResponseGet(webRequest);
            }

            return responseData;

        }

        /// <summary>
        /// Process the web response.
        /// </summary>
        /// <param name="webRequest">The request object.</param>
        /// <returns>The response data.</returns>
        public string WebResponseGet(HttpWebRequest webRequest)
        {
            StreamReader responseReader = null;
            string responseData = null;

            try
            {
                if (webRequest != null) responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
                if (responseReader != null) responseData = responseReader.ReadToEnd();
            }
            finally
            {
                if (webRequest != null)
                {
                    var responseStream = webRequest.GetResponse().GetResponseStream();
                    if (responseStream != null)
                        responseStream.Close();
                }
                if (responseReader != null) responseReader.Close();
            }

            return responseData;
        }
    }
}