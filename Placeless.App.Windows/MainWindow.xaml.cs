using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly BackgroundWorker collectWorker = new BackgroundWorker();
        private readonly BackgroundWorker generateWorker = new BackgroundWorker();

        public MainWindow()
        {
            _servicecollection = new ServiceCollection();
            string settingsFile = "appsettings.json";

#if DEBUG
            settingsFile = "appsettings.Development.json";
#endif


            _config = new SettingsBasedConfig();


            _servicecollection.AddSingleton<IUserInteraction>(this);
            _servicecollection.AddSingleton<IPlacelessconfig>(_config);
            _servicecollection.AddTransient<IMetadataStore, SqlMetadataStore>();
            _servicecollection.AddTransient<FlickrSource>();
            _servicecollection.AddTransient<WindowsSource>();
            _servicecollection.AddTransient<Placeless.Generator.Generator>();

            _servicecollection.AddTransient<Collector<FlickrSource>>();
            _servicecollection.AddTransient<Collector<WindowsSource>>();

            _serviceProvider = _servicecollection.BuildServiceProvider();
            collectWorker.DoWork += CollectWorker_DoWork;

            generateWorker.DoWork += GenerateWorker_DoWork;

            InitializeComponent();

            ProgressGroups = new ProgressReportGroup[]
            {
                new ProgressReportGroup { Category = progressCategory0, ProgressBar = progressBar0, ProgressBarText = progressBarText0 },
                new ProgressReportGroup { Category = progressCategory1, ProgressBar = progressBar1, ProgressBarText = progressBarText1 },
                new ProgressReportGroup { Category = progressCategory2, ProgressBar = progressBar2, ProgressBarText = progressBarText2 },
                new ProgressReportGroup { Category = progressCategory3, ProgressBar = progressBar3, ProgressBarText = progressBarText3 },
            };
        }

        private void GenerateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var generator = _serviceProvider.GetService<Placeless.Generator.Generator>();
            var generatorTask = generator.Generate(new CreatedYearAttributeGenerator());

            while (!generatorTask.Wait(1000))
            {
                ReportProgress(generator);
            }
            ReportProgress(generator);

        }

        private void CollectWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var windowsCollector = _serviceProvider.GetService<Collector<WindowsSource>>();

            var windowsTask = windowsCollector.Discover();
            while (!windowsTask.Wait(1000))
            {
                ReportProgress(windowsCollector);
            }
            ReportProgress(windowsCollector);

            var flickrCollector = _serviceProvider.GetService<Collector<FlickrSource>>();

            var flickrTask = flickrCollector.Discover();
            while (!flickrTask.Wait(1000))
            {
                ReportProgress(flickrCollector);
            }
            ReportProgress(flickrCollector);

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

        private void btnCollect_Click(object sender, RoutedEventArgs e)
        {
            collectWorker.RunWorkerAsync();
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

            int i = 0;
            foreach (var report in reporter.GetReports())
            {
                if (report.Max > 0)
                {
                    ProgressGroups[i].Category.Text = report.Category;
                    ProgressGroups[i].Category.Visibility = Visibility.Visible;
                    ProgressGroups[i].ProgressBar.Maximum = report.Max;
                    ProgressGroups[i].ProgressBar.Value = report.Current;
                    ProgressGroups[i].ProgressBar.Visibility = Visibility.Visible;
                    ProgressGroups[i].ProgressBarText.Text = $"{report.Current}/{report.Max}";
                    ProgressGroups[i].ProgressBarText.Visibility = Visibility.Visible;
                }
                i++;
            }
            while (i < 3)
            {
                ProgressGroups[i].Category.Visibility = Visibility.Collapsed;
                ProgressGroups[i].ProgressBar.Visibility = Visibility.Collapsed;
                ProgressGroups[i].ProgressBarText.Visibility = Visibility.Collapsed;
                i++;
            }
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
            generateWorker.RunWorkerAsync();
        }

        private void btnConfigureFlickr_Click(object sender, RoutedEventArgs e)
        {
            var settings = new FlickrSettings(_config);
            settings.ShowDialog();

            var db = _serviceProvider.GetService<IMetadataStore>();
            var page = new PhotoViewer(db);
            mainFrame.Navigate(page);

        }

        private void btnGenerateThumbnails_Click(object sender, RoutedEventArgs e)
        {
           var task = new Task(() =>
           {
               var generator = _serviceProvider.GetService<Placeless.Generator.Generator>();
               var generatorTask = generator.Generate(new MediumThumbnailGenerator());

               while (!generatorTask.Wait(1000))
               {
                   ReportProgress(generator);
               }
               ReportProgress(generator);
           });
            task.Start();
        }
    }
}
