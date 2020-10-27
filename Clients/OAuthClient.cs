using EP94.LgSmartThinq.Models;
using EP94.LgSmartThinq.Utils;
using Newtonsoft.Json;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EP94.LgSmartThinq.Clients
{
    internal class OAuthClient
    {
        private string _username;
        private string _password;
        private string _country;
        private string _languageCode;
        public OAuthClient(string username, string password, string country, string languageCode)
        {
            _username = username;
            _password = password;
            _country = country;
            _languageCode = languageCode;
        }

        public async Task<Passport> GetPassport()
        {
            string passportPath = 
                Path.Combine(
                    Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), string.Concat(nameof(Passport), ".json"));
            Passport passport = null;
            if (File.Exists(passportPath))
            {
                passport = JsonConvert.DeserializeObject<Passport>(File.ReadAllText(passportPath));
                await RefreshOAuthToken(passport);
            }
            else
            {
                string loginCode = await GetLoginCode();
                OAuthToken oAuthToken = await GetOAuthToken(loginCode);
                UserProfile userProfile = await GetUserProfile(oAuthToken);
                passport = new Passport()
                {
                    Token = oAuthToken,
                    UserProfile = userProfile,
                    Country = _country,
                    Language = _languageCode
                };
            }
            File.WriteAllText(passportPath, JsonConvert.SerializeObject(passport));
            return passport;
        }

        public async Task RefreshOAuthToken(Passport passport)
        {
            HttpClient client = new HttpClient();
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("grant_type", "refresh_token");
            queryParams.Add("refresh_token", passport.Token.RefreshToken);
            FormUrlEncodedContent formUrlEncoded = new FormUrlEncodedContent(queryParams);
            string urlEncodedString = formUrlEncoded.ReadAsStringAsync().Result;
            string relativeUrl = $"/oauth/1.0/oauth2/token?{urlEncodedString}";
            string fullUrl = $"{Constants.API_BASE_URL}{relativeUrl}";

            using var requestMessage = GetOAuthRequestMessage(fullUrl, relativeUrl, formUrlEncoded, HttpMethod.Post, true);
            var response = await client.SendAsync(requestMessage);
            string stringResponse = await response.Content.ReadAsStringAsync();
            OAuthToken newToken = JsonConvert.DeserializeObject<OAuthToken>(await response.Content.ReadAsStringAsync());
            passport.Token.AccessToken = newToken.AccessToken;
        }

        private async Task<string> GetLoginCode()
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>
            {
                { "country", _country },
                { "language", _languageCode },
                { "svc_list", Constants.SERVICE_CODE },
                { "client_id", Constants.LGE_APP_KEY },
                { "division", Constants.DIVISION },
                { "redirect_uri", Constants.OAUTH_REDIRECT_URI },
                { "state", "asdfasdf" },
                { "show_thirdparty_login", Constants.THIRD_PARTY_LOGINS }
            };
            FormUrlEncodedContent formUrlEncoded = new FormUrlEncodedContent(queryParams);
            string urlEncodedString = formUrlEncoded.ReadAsStringAsync().Result;
            string loginUrl = $"{string.Format(Constants.LOGIN_BASE_URL, _country)}/spx/login/signIn?{urlEncodedString}";
            BrowserFetcher browserFetcher = new BrowserFetcher();
            bool hasBrowserRevision = browserFetcher.LocalRevisions().FirstOrDefault((r) => r == BrowserFetcher.DefaultRevision) != default;
            if (!hasBrowserRevision)
            {
                Console.WriteLine("Downloading browser revision...");
                await browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision);
            }
            Browser browser = await Puppeteer.LaunchAsync(new LaunchOptions()
            {
                Headless = true
            });
            Page page = await browser.NewPageAsync();
            await page.GoToAsync(loginUrl);
            ElementHandle emailIdInput = await page.WaitForSelectorAsync("#user_id");
            ElementHandle passwordInput = await page.WaitForSelectorAsync("#user_pw");
            await emailIdInput.FocusAsync();
            await page.Keyboard.TypeAsync(_username);
            await passwordInput.FocusAsync();
            await page.Keyboard.TypeAsync(_password);
            await page.ClickAsync("#btn_login");
            await page.WaitForNavigationAsync();
            string responseUrl = page.Url;
            NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(responseUrl);
            return nameValueCollection.Get("code");
        }

        private async Task<OAuthToken> GetOAuthToken(string loginCode)
        {
            HttpClient client = new HttpClient();
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("code", loginCode);
            queryParams.Add("grant_type", "authorization_code");
            queryParams.Add("redirect_uri", Constants.OAUTH_REDIRECT_URI);
            FormUrlEncodedContent formUrlEncoded = new FormUrlEncodedContent(queryParams);
            string urlEncodedString = formUrlEncoded.ReadAsStringAsync().Result;
            string relativeUrl = $"/oauth/1.0/oauth2/token?{urlEncodedString}";
            string fullUrl = $"{Constants.API_BASE_URL}{relativeUrl}";

            using var requestMessage = GetOAuthRequestMessage(fullUrl, relativeUrl, formUrlEncoded, HttpMethod.Post, true);
            var response = await client.SendAsync(requestMessage);
            return JsonConvert.DeserializeObject<OAuthToken>(await response.Content.ReadAsStringAsync());
        }

        private async Task<UserProfile> GetUserProfile(OAuthToken token)
        {
            HttpClient client = new HttpClient();
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("access_code", token.AccessToken);
            FormUrlEncodedContent formUrlEncoded = new FormUrlEncodedContent(queryParams);
            string urlEncodedString = formUrlEncoded.ReadAsStringAsync().Result;
            string relativeUrl = $"/oauth/1.0/users/profile?{urlEncodedString}";
            string fullUrl = $"{Constants.API_BASE_URL}{relativeUrl}";
            using var requestMessage = GetOAuthRequestMessage(fullUrl, relativeUrl, formUrlEncoded, HttpMethod.Get, false);
            requestMessage.Headers.Add("Authorization", $"Bearer {token.AccessToken}");
            var response = await client.SendAsync(requestMessage);
            string responseString = await response.Content.ReadAsStringAsync();
            var jsonObject = JsonConvert.DeserializeObject<dynamic>(responseString);
            UserProfile userProfile = new UserProfile
            {
                UserNo = (string)jsonObject["account"]["userNo"],
                UserId = (string)jsonObject["account"]["userID"]
            };
            return userProfile;
        }

        private HttpRequestMessage GetOAuthRequestMessage(string fullUrl, string relativeUrl, FormUrlEncodedContent formUrlEncodedContent, HttpMethod httpMethod, bool useFormUrlEncodedContent)
        {
            var requestMessage = new HttpRequestMessage(httpMethod, fullUrl);
            string timestamp = DateTime.UtcNow.ToString(Constants.OAUTH_TIMESTAMP_FORMAT, CultureInfo.InvariantCulture);
            byte[] secret = Encoding.UTF8.GetBytes(Constants.OAUTH_SECRET);
            string messageString = $"{relativeUrl}\n{timestamp}";
            byte[] message = Encoding.UTF8.GetBytes(messageString);

            if (useFormUrlEncodedContent)
                requestMessage.Content = formUrlEncodedContent;

            byte[] hash = new HMACSHA1(secret).ComputeHash(message);
            string signature = Convert.ToBase64String(hash);

            requestMessage.Headers.Clear();
            requestMessage.Headers.Add("Accept", "application/json; charset=UTF-8");
            requestMessage.Headers.Add("x-lge-oauth-signature", signature);
            requestMessage.Headers.Add("x-lge-oauth-date", timestamp);
            requestMessage.Headers.Add("x-lge-appkey", Constants.LGE_APP_KEY);
            return requestMessage;
        }
    }
}
