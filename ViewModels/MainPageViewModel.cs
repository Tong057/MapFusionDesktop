using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MapFusion.Models;
using MapFusion.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace MapFusion.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        public MainPageViewModel(MainViewModel mainVM)
        {
            MainVM = mainVM;
        }

        [ObservableProperty]
        private MainViewModel _mainVM;

        [ObservableProperty]
        private ProxyModel _selectedProxy = new ProxyModel();

        [RelayCommand]
        private void OpenManageProxyWindow()
        {
            ManageProxyServersWindow window = new ManageProxyServersWindow(this);
            window.Show();
        }

        [RelayCommand]
        private void ConnectProxy()
        {
            if (AppSettings.ProxyList.Any())
            {
                MainVM.IsProxyConnected = true;
                AppSettings.UseProxy = true;
            }

        }

        [RelayCommand]
        private void DisconnectProxy()
        {
            MainVM.IsProxyConnected = false;
            AppSettings.UseProxy = false;
        }

        [RelayCommand]
        private void AddProxy()
        {
            AppSettings.ProxyList.Add(SelectedProxy);
            SelectedProxy = new ProxyModel();
        }

        [RelayCommand]
        private void DeleteProxy()
        {
            AppSettings.ProxyList.Remove(SelectedProxy);
            SelectedProxy = new ProxyModel();
        }
    }
}
