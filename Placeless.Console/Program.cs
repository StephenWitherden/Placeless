using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Placeless.Configuration.AspDotNet;
using Placeless.Generator;
using Placeless.Generator.Windows;
using Placeless.MetadataStore.Sql;
using Placeless.Source.Flickr;
using Placeless.Source.Windows;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Placeless.Console
{
    class Program 
    {
        static void Main(string[] args)
        {
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            var interaction = new ConsoleUserInteraction();
            interaction.ReportStatus("Placeless console app starting.");

            // add the framework services
            var services = new ServiceCollection()
                .AddLogging();

            if (!System.IO.File.Exists(Path.Combine(AppContext.BaseDirectory, "appsettings.json")))
            {
                interaction.ReportError("Missing config file: appsettings.json");
                return; // exit app
            }


            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", optional: false);

            var configuration = builder.Build();

            
            var config = new AspDotNetConfiguration(configuration);
            var store = new SqlMetadataStore(config, interaction);
            var source = new FlickrSource(store, config, interaction);

            ////var generator = new Generator.Generator(store);
            ////generator.Generate(new CreatedYearAttributeGenerator());
            ////generator.Generate(new MediumThumbnailGenerator());

            var collector = new Collector(store, source, interaction);

            var discoveryTask = collector.Discover();

            var rootPercent = collector.DiscoveredRoots == 0 ? 0 : 100 * collector.ProcessedRoots / collector.DiscoveredRoots;
            var filePercent = collector.DiscoveredFiles == 0 ? 0 : 100 * collector.ProcessedFiles / collector.DiscoveredFiles;

            while (!discoveryTask.Wait(1000))
            {
                rootPercent = collector.DiscoveredRoots == 0 ? 0 : 100 * collector.ProcessedRoots / collector.DiscoveredRoots;
                filePercent = collector.DiscoveredFiles == 0 ? 0 : 100 * collector.ProcessedFiles / collector.DiscoveredFiles;

                System.Console.WriteLine($"Roots: {collector.ProcessedRoots}/{collector.DiscoveredRoots} ({rootPercent}%); Files: {collector.ProcessedFiles}/{collector.DiscoveredFiles} ({filePercent}%)");
            }
            rootPercent = collector.DiscoveredRoots == 0 ? 0 : 100 * collector.ProcessedRoots / collector.DiscoveredRoots;
            filePercent = collector.DiscoveredFiles == 0 ? 0 : 100 * collector.ProcessedFiles / collector.DiscoveredFiles;
            System.Console.WriteLine($"Roots: {collector.ProcessedRoots}/{collector.DiscoveredRoots} ({rootPercent}%); Files: {collector.ProcessedFiles}/{collector.DiscoveredFiles} ({filePercent}%)");
        }


        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            System.Console.WriteLine(e.ExceptionObject.ToString());
        }
    }
}
