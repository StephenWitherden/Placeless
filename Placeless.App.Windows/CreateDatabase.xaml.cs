using Microsoft.Win32;
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
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Placeless.App.Windows
{
    /// <summary>
    /// Interaction logic for CreateDatabase.xaml
    /// </summary>
    public partial class CreateDatabase : Window
    {
        private readonly string _rootPath;

        public CreateDatabase(string rootPath)
        {
            _rootPath = rootPath;
            InitializeComponent();
        }

        public string DatabaseFile
        {
            get
            {
                return txtDatabaseFile.Text;
            }
        }

        public string DatabaseName
        {
            get
            {
                return txtDatabaseName.Text;
            }
        }

        public string FileFolder
        {
            get
            {
                return txtFileFolder.Text;
            }
        }

        public string LogFile
        {
            get
            {
                return txtLogFile.Text;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void btnDatabaseFile_Click(object sender, RoutedEventArgs e)
        {
            var selectFile = new SaveFileDialog { DefaultExt = "mdf", CheckFileExists = false, Filter = "Lock Files (*.mdf) | *.mdf" };
            if (selectFile.ShowDialog().GetValueOrDefault())
            {
                txtDatabaseFile.Text = selectFile.FileName;
            }
        }

        private void btnLogFile_Click(object sender, RoutedEventArgs e)
        {
            var selectFile = new SaveFileDialog { DefaultExt = "ldf", CheckFileExists = false, Filter = "Lock Files (*.ldf) | *.ldf" };
            if (selectFile.ShowDialog().GetValueOrDefault())
            {
                txtLogFile.Text = selectFile.FileName;
            }
        }

        private void btnFileFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                txtFileFolder.Text = dialog.FileName;
            }
        }

        private void TxtDatabaseName_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtDatabaseFile.Text = System.IO.Path.Combine(_rootPath, txtDatabaseName.Text+ ".mdf");
            txtLogFile.Text = System.IO.Path.Combine(_rootPath, txtDatabaseName.Text+ ".ldf");
            txtFileFolder.Text = System.IO.Path.Combine(_rootPath, txtDatabaseName.Text);
        }
    }
}
