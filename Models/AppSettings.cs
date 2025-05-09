using System.Collections.ObjectModel;
using System.IO;

namespace MapFusion.Models
{
    public class AppSettings
    {
        private static readonly string settingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MapFusion",
            "appsettings.txt");

        public static ObservableCollection<ProxyModel> ProxyList { get; set; } = new ObservableCollection<ProxyModel>();
        public static bool UseProxy { get; set; }
        public static string ActivationKey { get; set; }

        private static string _userName;
        public static string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        static AppSettings()
        {
            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(settingsFilePath));
        }

        public static void SaveSettings()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(settingsFilePath))
                {
                    writer.WriteLine($"ActivationKey={ActivationKey}");
                    writer.WriteLine($"UserName={UserName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public static bool LoadSettings()
        {
            if (!File.Exists(settingsFilePath))
                return false;

            try
            {
                using (StreamReader reader = new StreamReader(settingsFilePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        int separatorIndex = line.IndexOf('=');
                        if (separatorIndex != -1)
                        {
                            string key = line.Substring(0, separatorIndex);
                            string value = line.Substring(separatorIndex + 1);

                            switch (key)
                            {
                                case "ActivationKey":
                                    ActivationKey = value;
                                    break;
                                case "UserName":
                                    UserName = value;
                                    break;
                                    // Add more cases if you have additional settings
                            }
                        }
                    }
                }

                // Check if ActivationKey was successfully loaded
                if (ActivationKey == null)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                // Handle any errors during settings loading (e.g., write to log or display error message)
                Console.WriteLine($"Error loading settings: {ex.Message}");
                return false;
            }
        }

        public static void Logout()
        {
            try
            {
                if (File.Exists(settingsFilePath))
                {
                    File.Delete(settingsFilePath);
                    ActivationKey = null;
                    UserName = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging out: {ex.Message}");
            }
        }
    }
}

