using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Placeless.BlobStore.FileSystem;
using Placeless.Configuration.AspDotNet;
using Placeless.Generator;
using Placeless.Generator.Windows;
using Placeless.MetadataStore.Sql;
using Placeless.Source.Flickr;
using Placeless.Source.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
using Generator = Placeless.Generator.Generator;

namespace Placeless.App.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IUserInteraction
    {
        private readonly IServiceCollection _servicecollection;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPlacelessconfig _config;

        public MainWindow()
        {
            _servicecollection = new ServiceCollection();

            _config = new SettingsBasedConfig();


            _servicecollection.AddSingleton<IUserInteraction>(this);
            _servicecollection.AddSingleton<IPlacelessconfig>(_config);
            _servicecollection.AddTransient<IBlobStore, FileSystemBlobStore>();
            _servicecollection.AddTransient<IMetadataStore, SqlMetadataStore>();
            _servicecollection.AddTransient<FlickrSource>();
            _servicecollection.AddTransient<WindowsSource>();
            _servicecollection.AddTransient<Placeless.Generator.Generator>();

            _servicecollection.AddTransient<Collector<FlickrSource>>();
            _servicecollection.AddTransient<Collector<WindowsSource>>();

            _serviceProvider = _servicecollection.BuildServiceProvider();

            InitializeComponent();

            if (string.IsNullOrWhiteSpace(_config.GetValue(SqlMetadataStore.CONNECTION_STRING_SETTING)))
            {
                var wizard = new FirstTimeWizard(_config);
                wizard.ShowDialog();
            }

            ProgressGroups = new ProgressReportGroup[]
            {
                new ProgressReportGroup { Category = progressCategory0, ProgressBar = progressBar0, ProgressBarText = progressBarText0 },
                new ProgressReportGroup { Category = progressCategory1, ProgressBar = progressBar1, ProgressBarText = progressBarText1 },
                new ProgressReportGroup { Category = progressCategory2, ProgressBar = progressBar2, ProgressBarText = progressBarText2 },
                new ProgressReportGroup { Category = progressCategory3, ProgressBar = progressBar3, ProgressBarText = progressBarText3 },
                new ProgressReportGroup { Category = progressCategory4, ProgressBar = progressBar4, ProgressBarText = progressBarText4 },
                new ProgressReportGroup { Category = progressCategory5, ProgressBar = progressBar5, ProgressBarText = progressBarText5 },
            };
        }

        public string InputPrompt(string prompt)
        {
            if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
            {
                string result = "";
                Dispatcher.Invoke( () =>
                {
                    result = InputPrompt(prompt);
                });
                return result;
            }

            var inputBox = new InputBox(prompt);
            inputBox.ShowDialog();
            return inputBox.txtInput.Text;
        }

        public void OpenWebPage(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        public void ReportError(string error)
        {
            //throw new NotImplementedException();
        }

        public void ReportStatus(string status)
        {
            //throw new NotImplementedException();
        }

        public ProgressReportGroup[] ProgressGroups { get; set; }

        delegate void ProgressReporterInvoker(IProgressReporter reporter);

        private void ReportProgress(IProgressReporter reporter)
        {
            if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
            {
                Dispatcher.Invoke(new ProgressReporterInvoker(ReportProgress), reporter);
                return;
            }

            foreach (var report in reporter.GetReports())
            {
                var i = findIndexFor(report.Category);
                if (report.Max > 0 && report.Max != report.Current)
                {
                    ProgressGroups[i].Category.Text = report.Category;
                    ProgressGroups[i].Category.Visibility = Visibility.Visible;
                    ProgressGroups[i].ProgressBar.Maximum = report.Max;
                    ProgressGroups[i].ProgressBar.Value = report.Current;
                    ProgressGroups[i].ProgressBar.Visibility = Visibility.Visible;
                    ProgressGroups[i].ProgressBarText.Text = $"{report.Current}/{report.Max}";
                    ProgressGroups[i].ProgressBarText.Visibility = Visibility.Visible;

                }
                if (report.Max == report.Current)
                {
                    ProgressGroups[i].Category.Text = "";
                    ProgressGroups[i].Category.Visibility = Visibility.Collapsed;
                    ProgressGroups[i].ProgressBar.Visibility = Visibility.Collapsed;
                    ProgressGroups[i].ProgressBarText.Visibility = Visibility.Collapsed;
                }
            }
        }

        private int findIndexFor(string category)
        {
            for (int i = 0; i < ProgressGroups.Length; i++)
            {
                if (ProgressGroups[i].Category.Text == category)
                {
                    return i;
                }
            }

            // find first non-zero or non-complete task
            for (int i = 0; i < ProgressGroups.Length; i++)
            {
                if (ProgressGroups[i].Category.Text == "" || ProgressGroups[i].ProgressBar.Visibility == Visibility.Collapsed)
                {
                    return i;
                }
            }

            // default to the last
            return ProgressGroups.Length - 1;
        }

        private void btnConfigureFileSystem_Click(object sender, RoutedEventArgs e)
        {
            var settings = new WindowsSettings(_config);
            settings.ShowDialog();
        }

        private void btnChooseDatabase_Click(object sender, RoutedEventArgs e)
        {
            var databaseSettings = new ConnectionString(_config);
            databaseSettings.ShowDialog();
        }

        private void btnDeriveAttributes_Click(object sender, RoutedEventArgs e)
        {
            var attributeGenerator = _serviceProvider.GetService<Placeless.Generator.Generator>();
            attributeGenerator.Init(new CreatedYearAttributeGenerator());
            watch(attributeGenerator).Start();


            var generator = _serviceProvider.GetService<Placeless.Generator.Generator>();
            generator.Init(new FlickrPhotosetAttributeGenerator());
            watch(generator).Start();
        }

        private void btnGenerateThumbnails_Click(object sender, RoutedEventArgs e)
        {
            var generator = _serviceProvider.GetService<Placeless.Generator.Generator>();
            generator.Init(new MediumThumbnailGenerator());
            watch(generator).Start();
        }

        private Task watch(IProgressReporter job)
        {
            Task watcher = new Task(() =>
            {
               var task = job.DoWork();
               while (!task.Wait(1000))
               {
                   ReportProgress(job);
               }
               ReportProgress(job);
            }, TaskCreationOptions.LongRunning);
            return watcher;
        }

        private void refreshMetadata()
        {
            var attributeGenerator = _serviceProvider.GetService<Placeless.Generator.Generator>();
            attributeGenerator.Init(new CreatedYearAttributeGenerator());
            watch(attributeGenerator).Start();

            var generator = _serviceProvider.GetService<Placeless.Generator.Generator>();
            generator.Init(new FlickrPhotosetAttributeGenerator());
            watch(generator).Start();

            var thumbnailGenerator = _serviceProvider.GetService<Placeless.Generator.Generator>();
            thumbnailGenerator.Init(new MediumThumbnailGenerator());
            watch(thumbnailGenerator).Start();
        }

        private void btnCollectFlickr_Click(object sender, RoutedEventArgs e)
        {
            var collector = _serviceProvider.GetService<Collector<FlickrSource>>();

            var watchTask = watch(collector);

            watchTask
                .ContinueWith((t) =>
               {
                   refreshMetadata();
               });

            watchTask.Start();
        }

        private void btnCollectFolders_Click(object sender, RoutedEventArgs e)
        {
            var collector = _serviceProvider.GetService<Collector<WindowsSource>>();
            watch(collector);
        }

        private void btnViewGallery_Click(object sender, RoutedEventArgs e)
        {
            var db = _serviceProvider.GetService<IMetadataStore>();
            var page = new PhotoViewer(db);
            mainFrame.Navigate(page);
        }
    }
}
