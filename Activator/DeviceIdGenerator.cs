using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace MapFusion.Activator
{
    public static class DeviceIdGenerator
    {
        private const string EncryptionKey = "cinazes";

        public static string GetEncryptedDeviceId()
        {
            string deviceId = GetHardwareId();
            if (string.IsNullOrEmpty(deviceId))
            {
                deviceId = Guid.NewGuid().ToString();
            }
            return Encrypt(deviceId);
        }

        public static string DecryptDeviceId(string encryptedDeviceId)
        {
            return Decrypt(encryptedDeviceId);
        }

        private static string GetHardwareId()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive");
                foreach (ManagementObject wmi_HD in searcher.Get())
                {
                    return wmi_HD["SerialNumber"].ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения идентификатора устройства: {ex.Message}");
            }
            return null;
        }

        private static string Encrypt(string text)
        {
            using (Aes aes = Aes.Create())
            {
                var key = new Rfc2898DeriveBytes(EncryptionKey, Encoding.UTF8.GetBytes("SaltIsGoodForYou"));
                aes.Key = key.GetBytes(32);
                aes.IV = key.GetBytes(16);
                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    using (var ms = new System.IO.MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            using (var sw = new System.IO.StreamWriter(cs))
                            {
                                sw.Write(text);
                            }
                            return Convert.ToBase64String(ms.ToArray());
                        }
                    }
                }
            }
        }

        private static string Decrypt(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                var key = new Rfc2898DeriveBytes(EncryptionKey, Encoding.UTF8.GetBytes("SaltIsGoodForYou"));
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
}
