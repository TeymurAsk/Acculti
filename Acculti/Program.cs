string clientId = "1329846684594733106";
string redirectUri = "http://localhost/acculti-auth";
string scope = "identify";
string authUrl = $"https://discord.com/api/oauth2/authorize?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope={scope}";
Console.WriteLine("Open the following URL in your browser and authenticate:");
Console.WriteLine(authUrl);
