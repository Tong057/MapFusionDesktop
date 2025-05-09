using MapFusion.ViewModels;
using System.Windows.Controls;


namespace MapFusion.Views
{
    /// <summary>
    /// Interaction logic for YandexMapsPageUserControl.xaml
    /// </summary>
    public partial class YandexMapsPageUserControl : UserControl
    {
        public YandexMapsPageUserControl(MainViewModel mainVM)
        {
            InitializeComponent();

            DataContext = new YandexMapsPageViewModel(mainVM);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OutputTextBox.ScrollToEnd();
        }
    }
}
