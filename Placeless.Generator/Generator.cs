using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Placeless.Generator
{
    public class Generator : IProgressReporter
    {
        private readonly IMetadataStore _metadataStore;

        public Generator(IMetadataStore store)
        {
            _metadataStore = store;
        }

        string activity = "Generating";
        int max = 0;
        int processed = 0;

        public async Task Generate(AttributeGenerator generator)
        {
            activity = $"Generating " + generator.AttributeName;

            max = _metadataStore.CountFilesMissingAttribute(generator.AttributeName);
            processed = 0;

            var files = _metadataStore.FilesMissingAttribute(generator.AttributeName);

            foreach (var file in files)
            {
                string value = generator.GenerateAttribute(file);
                if (value != null)
                {
                    await _metadataStore.SetAttribute(file.Id, generator.AttributeName, value);
                }
                processed++;
            }
        }

        public async Task Generate(VersionGenerator generator)
        {
            activity = $"Generating " + generator.VersionTypeName;

            max = _metadataStore.CountFilesMissingAttributeVersion(generator.VersionTypeName);

            var files = _metadataStore.FilesMissingAttributeVersion(generator.VersionTypeName);
            processed = 0;

            foreach (var file in files)
            {
                try
                {
                    Stream content = _metadataStore.GetFileStream(file.Id);
                    var thumbnailValue = generator.GenerateVersion(content);
                    await _metadataStore.AddVersion(file.Id, generator.VersionTypeName, thumbnailValue);
                    processed++;
                }
                catch (Exception ex)
                {

                }
            }
        }

        public IEnumerable<ProgressReport> GetReports()
        {
            return new ProgressReport[]
            {
                new ProgressReport { Category = activity, Current = processed, Max = max }
            };
        }
    }
}
