using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MetadataExtractor;
using Newtonsoft.Json.Linq;
using Placeless;
using Placeless.Source.Windows;
using Directory = System.IO.Directory;


namespace Placeless.Source.Windows
{
    public class WindowsSource : ISource
    {
        private readonly IMetadataStore _metadataStore;
        private readonly IPlacelessconfig _configuration;
        private HashSet<string> _existingSources;

        private List<Enum> enums = new List<Enum>();

        public WindowsSource(IMetadataStore store, IPlacelessconfig configuration)
        {
            _metadataStore = store;
            _configuration = configuration;
            foreach (Enum enumValue in Enum.GetValues(typeof(FileAttributes)))
            {
                enums.Add(enumValue);
            }
        }


        public void RefreshMetadata(string path, IEnumerable<string> extentions)
        {
            _existingSources = _metadataStore.ExistingSources(GetName(), path);
            foreach (var existingSource in _existingSources)
            {
                if (extentions.Contains(Path.GetExtension(existingSource).ToLower()))
                {
                    string metadata = GetMetadata(existingSource);
                    _metadataStore.UpdateMetadataForSource(existingSource, metadata);
                }
            }
        }

        public Task RefreshMetadata()
        {

            var paths = _configuration.GetValues("FileSystem:Paths")
                .Where(p => !(string.IsNullOrWhiteSpace(p)));

            var fileTypes = _configuration.GetValues("FileSystem:Extensions")
                .Where(f => !(string.IsNullOrWhiteSpace(f)));

            foreach (var path in paths)
            {
                RefreshMetadata(path, fileTypes);
            }
            return Task.CompletedTask;
        }


        public Task Discover()
        {

            var paths = _configuration.GetValues("FileSystem:Paths")
                .Where(p => !(string.IsNullOrWhiteSpace(p)));

            var fileTypes = _configuration.GetValues("FileSystem:Extensions")
                .Where(f => !(string.IsNullOrWhiteSpace(f)));

            foreach (var path in paths)
            {
                _existingSources = _metadataStore.ExistingSources(GetName(), path);
                Discover(path, fileTypes);
            }
            return Task.CompletedTask;
        }

        private string GetMetadata(string path)
        {
            JObject j = new JObject();

            string attributes = ExpandAttributes(System.IO.File.GetAttributes(path));

            j.Add("Windows Created Utc", System.IO.File.GetCreationTimeUtc(path));
            j.Add("Windows Modified Utc", System.IO.File.GetLastWriteTimeUtc(path));
            j.Add("Windows Attributes", attributes);

            try
            {
                var metadataDir = ImageMetadataReader.ReadMetadata(path);
                var tags = metadataDir.SelectMany(d => d.Tags).Select(t => new { Name = Clean(t.DirectoryName + "_" + t.Name), Value = t.Description });
                foreach (var tag in tags)
                {
                    if (!j.ContainsKey(tag.Name))
                    {
                        j.Add(tag.Name, tag.Value);
                    }
                    else if (j[tag.Name].ToString() != tag.Value)
                    {
                        int i = 2;
                        string name = tag.Name + "_" + i.ToString();
                        while (j.ContainsKey(name))
                        {
                            i++;
                            name = tag.Name + "_" + i.ToString();
                        }
                        j.Add(name, tag.Value);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return j.ToString();
        }

        private string ExpandAttributes(FileAttributes fileAttributes)
        {
            var activeFlags = enums.Where(e => fileAttributes.HasFlag(e)).Select(e => Enum.GetName(typeof(FileAttributes), e));
            return string.Join(',', activeFlags);
        }

        private string Clean(string name)
        {
            return name.Replace(' ', '_').Replace('/', '_').Replace('-', '_');
        }

        private void Discover(string path, IEnumerable<string> extentions)
        {
            foreach (var filePath in Directory.EnumerateFiles(path))
            {
                if (extentions.Contains(Path.GetExtension(filePath).ToLower()) &&
                    !_existingSources.Contains(filePath)
                    )
                {
                    var stream = System.IO.File.OpenRead(filePath);
                    string metadata = GetMetadata(filePath);
                    _metadataStore.AddDiscoveredFile(stream, Path.GetFileName(filePath), filePath, metadata, GetName());
                }
            }

            foreach (var subPath in Directory.EnumerateDirectories(path))
            {
                try
                {
                    Discover(subPath, extentions);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public Task Retrieve()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "Windows";
        }
    }
}
