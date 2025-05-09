using MapFusion.ViewModels;
using Microsoft.Web.WebView2.Core;
using System.Windows;
using System.Windows.Controls;


namespace MapFusion.Views
{
    /// <summary>
    /// Interaction logic for GoogleMapsPageUserControl.xaml
    /// </summary>
    public partial class TwoGISMapsPageUserControl : UserControl
    {
        public TwoGISMapsPageUserControl(MainViewModel mainVM)
        {
            InitializeComponent();

            DataContext = new TwoGISMapsPageViewModel(mainVM);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OutputTextBox.ScrollToEnd();
        }
    }
}
