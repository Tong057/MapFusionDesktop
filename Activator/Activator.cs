using System.Security.Cryptography;
using System.Text;

public static class Activator
{
    private const string SecretCode = "MapFusion14058";
    private const string EncryptionKey = "cinazes";
    private const string DecryptionKey = "sancizes";

    public static bool ValidateActivationKey(string deviceId, string activationKey)
    {
        string decryptedActivationKey = Decrypt(activationKey, DecryptionKey);
        string expectedKey = Decrypt(deviceId, EncryptionKey) + SecretCode;
        return decryptedActivationKey == expectedKey;
    }

    private static string Decrypt(string cipherText, string decryptionKey)
    {
        using (Aes aes = Aes.Create())
        {
            var key = new Rfc2898DeriveBytes(decryptionKey, Encoding.UTF8.GetBytes("SaltIsGoodForYou"));
            aes.Key = key.GetBytes(32);
            aes.IV = key.GetBytes(16);
            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            {
                using (var ms = new System.IO.MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (var sr = new System.IO.StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
