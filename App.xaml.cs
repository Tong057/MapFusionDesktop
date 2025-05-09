using MapFusion.Activator;
using MapFusion.Models;
using MapFusion.Views;
using System.Configuration;
using System.Data;
using System.Windows;

namespace MapFusion
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            bool isActivated = false;
            if (AppSettings.LoadSettings())
            {
                isActivated = global::Activator.ValidateActivationKey(DeviceIdGenerator.GetEncryptedDeviceId(), AppSettings.ActivationKey);
            }

            if (isActivated)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                ActivationWindow actWindow = new ActivationWindow();
                actWindow.Show();
            }
        }
    }

}
