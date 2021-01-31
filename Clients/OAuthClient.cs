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
    public class OAuthClient
    {
        private string _username;
        private string _password;
        private string _country;
        private string _languageCode;
        private string _chromiumPath;
        public OAuthClient(string username, string password, string country, string languageCode, string chromiumPath = null)
        {
            _username = username;
            _password = password;
            _country = country;
            _languageCode = languageCode;
            _chromiumPath = chromiumPath;
        }

        public async Task<Passport> GetPassport()
        {
            string passportPath = 
                Path.Combine(
                    Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), string.Concat(nameof(Passport), ".json"));
            Passport passport = null;
            if (File.Exists(passportPath))
            {
                SmartThinqLogger.Log("Passport found on path {0}", LogLevel.Debug, passportPath);
                passport = JsonConvert.DeserializeObject<Passport>(File.ReadAllText(passportPath));
                await RefreshOAuthToken(passport);
            }
            else
            {
                SmartThinqLogger.Log("Passport not found, creating new", LogLevel.Debug);
                string loginCode = await GetLoginCode();
                if (loginCode == null)
                {
                    return null;
                }
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
            SmartThinqLogger.Log("Wrote passport to file {0}", LogLevel.Debug, passportPath);
            return passport;
        }

        public async Task RefreshOAuthToken(Passport passport)
        {
            SmartThinqLogger.Log("Refreshing OAuth token", LogLevel.Debug);
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
            SmartThinqLogger.Log("Refreshing OAuth token successful", LogLevel.Debug);
        }

        private async Task<string> GetLoginCode()
        {
            bool authenticationError = false;
            SmartThinqLogger.Log("Getting login code", LogLevel.Debug);
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
                SmartThinqLogger.Log("No chromium revision {0} found, downloading...", LogLevel.Debug, BrowserFetcher.DefaultRevision);
                await browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision);
            }
            LaunchOptions launchOptions = new LaunchOptions()
            {
                Headless = true,
                Args = new string[] { "--no-sandbox" }
            };
            if (_chromiumPath != null)
                launchOptions.ExecutablePath = _chromiumPath;
            Browser browser = await Puppeteer.LaunchAsync(launchOptions);
            SmartThinqLogger.Log("Launching browser successful", LogLevel.Debug);
            Page page = await browser.NewPageAsync();
            page.Dialog += async (sender, args) =>
            {
                SmartThinqLogger.Log("Too many failed login attempts, please login at {0} and login yourself and run again", LogLevel.Fatal, loginUrl);
                authenticationError = true;
                await args.Dialog.Dismiss();
            };
            await page.GoToAsync(loginUrl);
            ElementHandle emailIdInput = await page.WaitForSelectorAsync("#user_id");
            ElementHandle passwordInput = await page.WaitForSelectorAsync("#user_pw");
            await emailIdInput.FocusAsync();
            await page.Keyboard.TypeAsync(_username);
            await passwordInput.FocusAsync();
            await page.Keyboard.TypeAsync(_password);
            await page.ClickAsync("#btn_login");
            try
            {
                await page.WaitForNavigationAsync(new NavigationOptions() { Timeout = 2000 });
            }
            catch
            {
                SmartThinqLogger.Log("Failed to login", LogLevel.Error);
                if (page.Url.Contains("changePw"))
                {
                    SmartThinqLogger.Log("LG wants you to change your password, please login at {0} and login yourself and run again after changing password", LogLevel.Fatal, loginUrl);
                }
                else if (authenticationError)
                {
                    return null;
                }
                else
                {
                    ElementHandle error = null;
                    try { error = await page.WaitForSelectorAsync("#error_user_pw", new WaitForSelectorOptions() { Timeout = 2000 }); } 
                    catch
                    {
                        SmartThinqLogger.Log("Unexpected error occured, please login at {0} and login yourself and run again", LogLevel.Fatal, loginUrl);
                        return null;
                    }
                    var innerText = (await error.GetPropertyAsync("innerText")).ToString();
                    string errorMsg = innerText.Replace("JSHandle:", "");
                    SmartThinqLogger.Log("Authentication error: {0}", LogLevel.Fatal, errorMsg);
                }
                await browser.CloseAsync();
                return null;
            }
            string responseUrl = page.Url;
            NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(responseUrl);
            string code = nameValueCollection.Get("code");
            SmartThinqLogger.Log("Received code successful", LogLevel.Debug);
            return code;
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
