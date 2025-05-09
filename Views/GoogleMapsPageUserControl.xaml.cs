using MapFusion.ViewModels;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MapFusion.Views
{
    /// <summary>
    /// Interaction logic for GoogleMapsPageUserControl.xaml
    /// </summary>
    public partial class GoogleMapsPageUserControl : UserControl
    {
        public GoogleMapsPageUserControl(MainViewModel mainVM)
        {
            InitializeComponent();

            DataContext = new GoogleMapsPageViewModel(mainVM);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OutputTextBox.ScrollToEnd();
        }
    }

}
