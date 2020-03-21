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
        private AttributeGenerator _attributeGenerator;
        private VersionGenerator _versionGenerator;

        public Generator(IMetadataStore store)
        {
            _metadataStore = store;
        }

        public void Init(AttributeGenerator generator)
        {
            _attributeGenerator = generator;
        }

        public void Init(VersionGenerator generator)
        {
            _versionGenerator = generator;
        }

        string activity = "Generating";
        int max = 0;
        int processed = 0;



        private async Task Generate(AttributeGenerator generator)
        {
            activity = $"Generating " + generator.AttributeName;

            max = _metadataStore.CountFilesMissingAttribute(generator.AttributeName);
            processed = 0;

            var files = _metadataStore.FilesMissingAttribute(generator.AttributeName);

            foreach (var file in files)
            {
                var values = generator.GenerateAttribute(file);
                if (values != null)
                {
                    foreach (var value in values)
                    {
                        await _metadataStore.SetAttribute(file.Id, generator.AttributeName, value);
                    }
                }
                processed++;
            }
        }

        private async Task Generate(VersionGenerator generator)
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

        public async Task DoWork()
        {
            if (_versionGenerator != null)
            {
                await Generate(_versionGenerator); 
            }
            if (_attributeGenerator != null)
            {
                await Generate(_attributeGenerator);
            }
        }
    }
}
