using System;
using System.Collections.Concurrent;
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
        private readonly IEnumerable<string> _fileTypes;

        public const string PATHS_SETTING = "FileSystem:Paths";
        public const string EXTENSIONS_SETTING = "FileSystem:Extensions";

        private List<Enum> enums = new List<Enum>();

        public WindowsSource(IMetadataStore store, IPlacelessconfig configuration)
        {
            _metadataStore = store;
            _configuration = configuration;
            foreach (Enum enumValue in Enum.GetValues(typeof(FileAttributes)))
            {
                enums.Add(enumValue);
            }
            _fileTypes = _configuration.GetValues(EXTENSIONS_SETTING)
                .Where(f => !(string.IsNullOrWhiteSpace(f)))
                .ToList();
        }


        public IEnumerable<string> GetRoots()
        {
            ConcurrentQueue<string> inputs = new ConcurrentQueue<string>();
            var items = _configuration.GetValues(PATHS_SETTING)
                .Where(p => !(string.IsNullOrWhiteSpace(p)));

            foreach (var item in items)
            {
                yield return item;
                inputs.Enqueue(item);
            }

            string current = null;

            while (inputs.TryDequeue(out current))
            {
                var childItems = Directory.EnumerateDirectories(current);
                foreach (var item in childItems)
                {
                    yield return item;
                    inputs.Enqueue(item);
                }
            }
        }

        public async Task<string> GetMetadata(string path)
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

            return await Task.FromResult(j.ToString());
        }

        private string ExpandAttributes(FileAttributes fileAttributes)
        {
            var activeFlags = enums.Where(e => fileAttributes.HasFlag(e)).Select(e => Enum.GetName(typeof(FileAttributes), e));
            return string.Join(",", activeFlags);
        }

        private string Clean(string name)
        {
            return name.Replace(' ', '_').Replace('/', '_').Replace('-', '_');
        }

        public IEnumerable<DiscoveredFile> Discover(string path, HashSet<string> existingSources)
        {
            var files = _fileTypes.AsParallel().SelectMany(pattern => 
                Directory.EnumerateFiles(path, pattern, SearchOption.TopDirectoryOnly)
            );
            var unprocessedFiles = files.Where(f => !existingSources.Contains(f.ToLower()))
                .Select(filePath => new DiscoveredFile
                {
                    Name = Path.GetFileName(filePath),
                    Extension = System.IO.Path.GetExtension(filePath),
                    Path = filePath,
                    Url = filePath
                });

            return unprocessedFiles;
        }

        public Stream GetContents(DiscoveredFile file)
        {
            var stream = System.IO.File.OpenRead(file.Url);
            return stream;
        }

        public string GetName()
        {
            return "Windows";
        }

    }
}
