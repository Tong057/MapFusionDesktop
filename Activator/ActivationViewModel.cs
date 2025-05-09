using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MapFusion.Activator;
using MapFusion.Models;
using MapFusion.Views;
using MaterialDesignThemes.Wpf;

namespace MapFusion.ViewModels
{
    public partial class ActivationViewModel : ObservableObject
    {
        private ActivationWindow _activationWindow;
        public ActivationViewModel(ActivationWindow activationWindow)
        {
            _activationWindow = activationWindow;
            SnackbarMessageQueue = new SnackbarMessageQueue(TimeSpan.FromMilliseconds(1000));
            SerialNumber = DeviceIdGenerator.GetEncryptedDeviceId();
        }

        [ObservableProperty]
        private SnackbarMessageQueue _snackbarMessageQueue;

        [ObservableProperty]
        private string _serialNumber;

        [ObservableProperty]
        private string _userName;

        [ObservableProperty]
        private string _activationKey;

        [RelayCommand]
        private void Activate()
        {
            if (string.IsNullOrEmpty(UserName))
            {
                SnackbarMessageQueue.Enqueue("Поле с ником не может быть пустым.");
                return;
            }

            try
            {
                if (global::Activator.ValidateActivationKey(SerialNumber, ActivationKey))
                {
                    AppSettings.UserName = UserName;
                    AppSettings.ActivationKey = ActivationKey;
                    AppSettings.SaveSettings();

                    var mainWindow = new MainWindow();
                    mainWindow.Show();

                    _activationWindow.Close();
                }
            }
            catch
            {
                SnackbarMessageQueue.Enqueue("Ошибка активации.");
            }
        }
    }
}
