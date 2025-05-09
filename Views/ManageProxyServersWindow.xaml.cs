using Dark.Net;
using MapFusion.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MapFusion.Views
{
    /// <summary>
    /// Interaction logic for ManageProxyServersWindow.xaml
    /// </summary>
    public partial class ManageProxyServersWindow : Window
    {
        public ManageProxyServersWindow(MainPageViewModel mainPageVM)
        {
            InitializeComponent();
            DarkNet.Instance.SetWindowThemeWpf(this, Theme.Dark);
            DataContext = mainPageVM;
        }
    }
}
