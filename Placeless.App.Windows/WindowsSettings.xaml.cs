using Placeless.Source.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Placeless.App.Windows
{
    /// <summary>
    /// Interaction logic for WindowsSettings.xaml
    /// </summary>
    public partial class WindowsSettings : Window
    {
        private readonly IPlacelessconfig _config;

        public WindowsSettings(IPlacelessconfig config)
        {
            _config = config;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var paths = _config.GetValues(WindowsSource.PATHS_SETTING);
            txtPaths.Text = string.Join("\r\n", paths);

            var extensions = _config.GetValues(WindowsSource.EXTENSIONS_SETTING);
            txtExtensions.Text = string.Join("\r\n", extensions);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var paths = txtPaths.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            _config.SetValues(WindowsSource.PATHS_SETTING, paths);

            var extensions = txtExtensions.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            _config.SetValues(WindowsSource.EXTENSIONS_SETTING, extensions);

            Close();
        }
    }
}
