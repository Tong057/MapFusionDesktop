using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MapFusion.Models;
using MapFusion.Views;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace MapFusion.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private MainWindow _mainWindow;
        private UserControl _mainPage;
        private UserControl _googleMapsPage;
        private UserControl _yandexMapsPage;
        private UserControl _twoGISMapsPage;

        public MainViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            _mainPage = new MainPageUserControl(this);
            _googleMapsPage = new GoogleMapsPageUserControl(this);
            _yandexMapsPage = new YandexMapsPageUserControl(this);
            _twoGISMapsPage = new TwoGISMapsPageUserControl(this);

            CurrentPage = _mainPage;
        }


        [ObservableProperty]
        private bool _isBusy = false;

        [ObservableProperty]
        private bool _isProxyConnected = false;

        [ObservableProperty]
        private string _parseStatus = "0%";

        [ObservableProperty]
        private UserControl _currentPage;

        private ListViewItem _selectedMenuItem;
        public ListViewItem SelectedMenuItem
        {
            get => _selectedMenuItem;
            set
            {
                SetProperty(ref _selectedMenuItem, value);
                OnMenuItemSelected(value);
            }
        }

        private void OnMenuItemSelected(ListViewItem menuItem)
        {
            switch (menuItem.Name)
            {
                case "ItemHome":
                    CurrentPage = _mainPage;
                    break;

                case "ItemGoogle":
                    CurrentPage = _googleMapsPage;
                    break;

                case "ItemYandex":
                    CurrentPage = _yandexMapsPage;
                    break;

                case "Item2GIS":
                    CurrentPage = _twoGISMapsPage;
                    break;

                default:
                    return;
            }
        }

        [RelayCommand]
        private void LogOut()
        {
            AppSettings.Logout();

            var activationWindow = new ActivationWindow();
            activationWindow.Show();

            _mainWindow.Close();
        }

    }
}
