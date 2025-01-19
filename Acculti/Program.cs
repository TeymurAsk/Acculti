using Newtonsoft.Json;

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
