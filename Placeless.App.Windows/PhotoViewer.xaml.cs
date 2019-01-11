using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Placeless.App.Windows
{
    /// <summary>
    /// Interaction logic for PhotoViewer.xaml
    /// </summary>
    public partial class PhotoViewer : Page, INotifyPropertyChanged
    {
        public ObservableCollection<AttributeValue> RootValues { get; set; }
        public ObservableCollection<Thumbnail> Files { get; set; }

        private readonly IMetadataStore _metadataStore;

        public PhotoViewer(IMetadataStore metadataStore)
        {
            _metadataStore = metadataStore;

            InitializeComponent();
            RootValues = new ObservableCollection<AttributeValue>();
            Files = new ObservableCollection<Thumbnail>();
            tvAttributeValues.DataContext = this;
            lvPhotos.DataContext = this;
        }

        private File _selectedFile;
        public File SelectedFile
        {
            get { return _selectedFile; }
            set { _selectedFile = value; NotifyPropertyChanged("SelectedFile"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void refreshAttributes()
        {
        }

        private void tvAttributeValues_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var attributeValue = (e.NewValue as AttributeValue);
            var files = _metadataStore.ThumbnailsForAttributeValue(attributeValue.Id);
            Files.Clear();
            foreach (var file in files)
            {
                Files.Add(file);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var values = _metadataStore.AllAttributeValues("Year Created");
            RootValues.Clear();
            foreach (var value in values)
            {
                RootValues.Add(value);
            }
        }

    }
}
