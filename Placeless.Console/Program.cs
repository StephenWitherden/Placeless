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

namespace Placeless.Console
{
    class Program 
    {
        static void Main(string[] args)
        {
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

            source.Discover();
        }

        public string InputPrompt(string prompt)
        {
            throw new NotImplementedException();
        }

        public void OpenWebPage(string url)
        {
            throw new NotImplementedException();
        }
    }
}
