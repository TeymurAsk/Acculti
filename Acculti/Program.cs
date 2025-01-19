using Acculti;
using Newtonsoft.Json;
using System.Text.Json;

string clientId = "1329846684594733106";
string redirectUri = "http://localhost/acculti-auth";
string scope = "identify";
string authUrl = $"https://discord.com/api/oauth2/authorize?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope={scope}";
Console.WriteLine("Open the following URL in your browser and authenticate:");
Console.WriteLine(authUrl);
HttpClient client = new HttpClient();
var values = new Dictionary<string, string>
{
    { "client_id", "YOUR_CLIENT_ID" },
    { "client_secret", "YOUR_CLIENT_SECRET" },
    { "grant_type", "authorization_code" },
    { "code", "USER_PROVIDED_CODE" },
    { "redirect_uri", "http://localhost/cli-auth" }
};
var content = new FormUrlEncodedContent(values);
HttpResponseMessage response = await client.PostAsync("https://discord.com/api/oauth2/token", content);
string responseString = await response.Content.ReadAsStringAsync();
var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
string accessToken = tokenData["access_token"];
string refreshToken = tokenData["refresh_token"];
string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Acculti", "user_accounts.json");
void SaveAccounts(UserAccountStore accountStore)
{
    string directory = Path.GetDirectoryName(filePath);
    if (!Directory.Exists(directory))
        Directory.CreateDirectory(directory);

    string json = Newtonsoft.Json.JsonSerializer.Serialize(accountStore, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(filePath, json);
}
UserAccountStore LoadAccounts()
{
    if (!File.Exists(filePath))
        return new UserAccountStore();

    string json = File.ReadAllText(filePath);
    return Newtonsoft.Json.JsonSerializer.Deserialize<UserAccountStore>(json) ?? new UserAccountStore();
}
void AddAccount(UserAccountStore accountStore, UserAccount account)
{
    accountStore.Accounts.Add(account);
    SaveAccounts(accountStore);
}
void RemoveAccount(UserAccountStore accountStore, string nickname)
{
    var account = accountStore.Accounts.FirstOrDefault(a => a.Nickname == nickname);
    if (account != null)
    {
        accountStore.Accounts.Remove(account);
        SaveAccounts(accountStore);
    }
    else
    {
        Console.WriteLine($"Account with nickname '{nickname}' not found.");
    }
}
void UpdateAccount(UserAccountStore accountStore, string nickname, UserAccount updatedAccount)
{
    var account = accountStore.Accounts.FirstOrDefault(a => a.Nickname == nickname);
    if (account != null)
    {
        account.AccessToken = updatedAccount.AccessToken;
        account.RefreshToken = updatedAccount.RefreshToken;
        account.TokenExpiry = updatedAccount.TokenExpiry;
        SaveAccounts(accountStore);
    }
}

