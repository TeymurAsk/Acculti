using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Acculti
{
    public static class Methods
    {
        public static void SaveAccounts(UserAccountStore accountStore, string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(accountStore, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
        public static UserAccountStore LoadAccounts(string filePath)
        {
            if (!File.Exists(filePath))
                return new UserAccountStore();

            string json = File.ReadAllText(filePath);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<UserAccountStore>(json) ?? new UserAccountStore();
        }

        public static void AddAccount(UserAccountStore accountStore, UserAccount account, string filePath)
        {
            accountStore.Accounts.Add(account);
            SaveAccounts(accountStore, filePath);
        }
        public static void RemoveAccount(UserAccountStore accountStore, string nickname, string filePath)
        {
            var account = accountStore.Accounts.FirstOrDefault(a => a.Nickname == nickname);
            if (account != null)
            {
                accountStore.Accounts.Remove(account);
                SaveAccounts(accountStore, filePath);
            }
            else
            {
                Console.WriteLine($"Account with nickname '{nickname}' not found.");
            }
        }
        public static void UpdateAccount(UserAccountStore accountStore, string nickname, UserAccount updatedAccount, string filePath)
        {
            var account = accountStore.Accounts.FirstOrDefault(a => a.Nickname == nickname);
            if (account != null)
            {
                account.AccessToken = updatedAccount.AccessToken;
                account.RefreshToken = updatedAccount.RefreshToken;
                account.TokenExpiry = updatedAccount.TokenExpiry;
                SaveAccounts(accountStore, filePath);
            }
        }
        public static string CaptureAuthorizationCode(string redirectUri)
        {
            using (var httpListener = new HttpListener())
            {
                httpListener.Prefixes.Add(redirectUri + "/");
                httpListener.Start();
                Console.WriteLine("Waiting for authentication...");

                var context = httpListener.GetContext();
                var request = context.Request;

                string code = request.QueryString["code"];

                var response = context.Response;
                string responseString = "<html><body>Authentication complete. You can close this window.</body></html>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();

                httpListener.Stop();
                return code;
            }
        }
        public static async Task<Dictionary<string, string>> ExchangeCodeForTokensAsync(string code, string clientId, string clientSecret, string redirectUri)
        {
            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", redirectUri }
            };
            var content = new FormUrlEncodedContent(values);
            HttpResponseMessage response = await client.PostAsync("https://discord.com/api/oauth2/token", content);
            string responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
        }
    }
}
