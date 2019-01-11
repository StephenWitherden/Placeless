using Placeless.Source.Flickr;
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
    /// Interaction logic for FlickrSettings.xaml
    /// </summary>
    public partial class FlickrSettings : Window
    {
        private readonly IPlacelessconfig _config;

        public FlickrSettings(IPlacelessconfig config)
        {
            InitializeComponent();
            _config = config;
            cbFlickrEnabled.IsChecked = bool.Parse(_config.GetValue(FlickrSource.ENABLED_PATH));
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            _config.SetValue(FlickrSource.ENABLED_PATH, cbFlickrEnabled.IsChecked.GetValueOrDefault().ToString());
        }

        private void btnResetToken_Click(object sender, RoutedEventArgs e)
        {
            _config.SetValue(FlickrSource.TOKEN_PATH, "");
        }
    }
}
