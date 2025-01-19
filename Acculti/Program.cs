using Acculti;
using Newtonsoft.Json;


string clientId = "1329846684594733106";
string redirectUri = "http://localhost/acculti-auth";
string scope = "identify";
string authUrl = $"https://discord.com/api/oauth2/authorize?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope={scope}";
//Console.WriteLine("Open the following URL in your browser and authenticate:");
//Console.WriteLine(authUrl);
//HttpClient client = new HttpClient();
//var values = new Dictionary<string, string>
//{
//    { "client_id", clientId },
//    { "client_secret", "YOUR_CLIENT_SECRET" },
//    { "grant_type", "authorization_code" },
//    { "code", "USER_PROVIDED_CODE" },
//    { "redirect_uri", "http://localhost/cli-auth" }
//};
//var content = new FormUrlEncodedContent(values);
//HttpResponseMessage response = await client.PostAsync("https://discord.com/api/oauth2/token", content);
//string responseString = await response.Content.ReadAsStringAsync();
//var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
//string accessToken = tokenData["access_token"];
//string refreshToken = tokenData["refresh_token"];
string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Acculti", "user_accounts.json");


var accountManager = new UserAccountStore();
var accounts = Methods.LoadAccounts(filePath);

Console.WriteLine("Commands: add, list, remove, exit");
while (true)
{
    Console.Write("> ");
    string command = Console.ReadLine();
    switch (command)
    {
        case "add":
            Console.Write("Enter nickname: ");
            string nickname = Console.ReadLine();

            Console.WriteLine("Open the following URL in your browser to authenticate:");
            string authUrl = $"https://discord.com/api/oauth2/authorize?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope={scope}";
            Console.WriteLine(authUrl);

            string authorizationCode = CaptureAuthorizationCode(redirectUri);
            var tokenData = await ExchangeCodeForTokensAsync(authorizationCode, clientId, "YOUR_CLIENT_SECRET", redirectUri);

            string accessToken = tokenData["access_token"];
            string refreshToken = tokenData["refresh_token"];
            DateTime expiry = DateTime.UtcNow.AddSeconds(int.Parse(tokenData["expires_in"]));

            accounts.Accounts.Add(new UserAccount
            {
                Nickname = nickname,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenExpiry = expiry
            });
            Methods.SaveAccounts(accounts, filePath);
            Console.WriteLine("Account added!");
            break;
        //Console.Write("Enter nickname: ");
        //string nickname = Console.ReadLine();

        //accounts.Accounts.Add(new UserAccount
        //{
        //    Nickname = nickname,
        //    AccessToken = accessToken,
        //    RefreshToken = refreshToken,
        //    TokenExpiry = DateTime.UtcNow.AddHours(1)
        //});
        //Methods.SaveAccounts(accounts, filePath);
        //Console.WriteLine("Account added!");
        //break;
        case "list":
            Console.WriteLine("Stored accounts:");
            foreach (var account in accounts.Accounts)
                Console.WriteLine($"- {account.Nickname}");
            break;

        case "remove":
            Console.Write("Enter nickname to remove: ");
            string nameToRemove = Console.ReadLine();
            Methods.RemoveAccount(accounts, nameToRemove, filePath);
            break;

        case "exit":
            return;

        default:
            Console.WriteLine("Unknown command.");
            break;
    }
}