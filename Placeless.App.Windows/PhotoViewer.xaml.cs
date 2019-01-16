using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
        public ObservableCollection<string> Attributes { get; set; }
        public ObservableCollection<AttributeValue> RootValues { get; set; }
        public ObservableCollection<Thumbnail> Files { get; set; }

        private string _selectedAttribute;
        public string SelectedAttribute
        {
            get
            {
                return _selectedAttribute;
            }
            set
            {
                _selectedAttribute = value;
                var values = _metadataStore.AllAttributeValues(_selectedAttribute);
                RootValues.Clear();
                foreach (var v in values)
                {
                    RootValues.Add(v);
                }
            }
        }

        private readonly IMetadataStore _metadataStore;

        public PhotoViewer(IMetadataStore metadataStore)
        {
            _metadataStore = metadataStore;

            InitializeComponent();
            RootValues = new ObservableCollection<AttributeValue>();
            Files = new ObservableCollection<Thumbnail>();
            Attributes = new ObservableCollection<string>();

            var attributes = _metadataStore.AllAttributes();
            foreach(var attribute in attributes)
            {
                Attributes.Add(attribute);
            }
            SelectedAttribute = attributes.FirstOrDefault();

            cboAttributes.DataContext = this;
            tvAttributeValues.DataContext = this;
            lvPhotos.DataContext = this;
        }

        private DisplayFile _selectedFile;
        public DisplayFile SelectedFile
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

        }

        private void LvPhotos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var thumbnail = lvPhotos.SelectedItem as Thumbnail;

            if (thumbnail != null)
            {
                var stream = _metadataStore.GetFileStream(thumbnail.Fileid);
                var image = BitmapFrame.Create(stream,
                                                    BitmapCreateOptions.PreservePixelFormat,
                                                    BitmapCacheOption.OnLoad);

                imgPreview.Source = image;
                border.Reset();

                txtMetadata.Text = string.Join("\r\n\r\n", _metadataStore.GetMetadata(thumbnail.Fileid));
            }
        }

        private void btnExportAttribute_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var attributeValue = tvAttributeValues.SelectedItem as AttributeValue;
                var files = _metadataStore.FilesForAttributeValue(attributeValue.Id);

                foreach(var file in files)
                {
                    var stream = _metadataStore.GetFileStream(file.Id);
                    var path = System.IO.Path.Combine(dialog.FileName, file.Title + "_" + file.Id + file.Extension);
                    using (var fileStream = System.IO.File.OpenWrite(path))
                    {
                        stream.CopyTo(fileStream);
                        fileStream.Flush();
                    }
                }
            }

            
        }
    }
}
