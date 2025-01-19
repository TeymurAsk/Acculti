using Acculti;
using Newtonsoft.Json;


string clientId = Environment.GetEnvironmentVariable("DISCORD_APP_CLIENT_ID");
string clientSecret = Environment.GetEnvironmentVariable("DISCORD_APP_CLIENT_SECRET");
string redirectUri = "http://localhost/acculti-auth";
string scope = "identify";
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
        case "login":
            Console.WriteLine("Stored accounts:");
            for (int i = 0; i < accounts.Accounts.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {accounts.Accounts[i].Nickname}");
            }
            Console.Write("Enter the number of the account to login: ");
            int selectedAccountIndex = int.Parse(Console.ReadLine()) - 1;

            if (selectedAccountIndex >= 0 && selectedAccountIndex < accounts.Accounts.Count)
            {
                var selectedAccount = accounts.Accounts[selectedAccountIndex];
                if (selectedAccount.TokenExpiry < DateTime.UtcNow)
                {
                    Console.WriteLine("Token expired. Refreshing token...");
                    var refreshedTokens = await Methods.RefreshTokenAsync(selectedAccount.RefreshToken, clientId, clientSecret, redirectUri);

                    selectedAccount.AccessToken = refreshedTokens["access_token"];
                    selectedAccount.RefreshToken = refreshedTokens["refresh_token"];
                    selectedAccount.TokenExpiry = DateTime.UtcNow.AddSeconds(int.Parse(refreshedTokens["expires_in"]));

                    Methods.SaveAccounts(accounts, filePath);
                }

                Console.WriteLine($"Logging into Discord as {selectedAccount.Nickname}...");
                var userInfo = await Methods.GetUserInfoAsync(selectedAccount.AccessToken);
                Console.WriteLine($"Logged in as {userInfo.Username}#{userInfo.Discriminator}");
            }
            else
            {
                Console.WriteLine("Invalid account selection.");
            }
            break;
        case "add":
            Console.Write("Enter nickname: ");
            string nickname = Console.ReadLine();

            Console.WriteLine("Open the following URL in your browser to authenticate:");
            string authUrl = $"https://discord.com/api/oauth2/authorize?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope={scope}";
            Console.WriteLine(authUrl);

            string authorizationCode = Methods.CaptureAuthorizationCode(redirectUri);
            var tokenData = await Methods.ExchangeCodeForTokensAsync(authorizationCode, clientId, clientSecret, redirectUri);

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