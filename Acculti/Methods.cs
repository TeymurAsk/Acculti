using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Acculti
{
    public class Methods
    {
        public void SaveAccounts(UserAccountStore accountStore, string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(accountStore, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
        public UserAccountStore LoadAccounts(string filePath)
        {
            if (!File.Exists(filePath))
                return new UserAccountStore();

            string json = File.ReadAllText(filePath);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<UserAccountStore>(json) ?? new UserAccountStore();
        }

        public void AddAccount(UserAccountStore accountStore, UserAccount account, string filePath)
        {
            accountStore.Accounts.Add(account);
            SaveAccounts(accountStore, filePath);
        }
        public void RemoveAccount(UserAccountStore accountStore, string nickname, string filePath)
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
        public void UpdateAccount(UserAccountStore accountStore, string nickname, UserAccount updatedAccount, string filePath)
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
    }
}
