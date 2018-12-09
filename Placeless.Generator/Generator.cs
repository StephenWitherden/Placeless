using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Placeless.Generator
{
    public class Generator
    {
        private readonly IMetadataStore _metadataStore;

        public Generator(IMetadataStore store)
        {
            _metadataStore = store;
        }

        public void Generate(AttributeGenerator generator)
        {
            var files = _metadataStore.FilesMissingAttribute(generator.AttributeName);
            foreach (var file in files)
            {
                string value = generator.GenerateAttribute(file);
                if (value != null)
                {
                    _metadataStore.SetAttribute(file.Id, generator.AttributeName, value);
                }
            }
        }

        public void Generate(VersionGenerator generator)
        {
            var files = _metadataStore.FilesMissingAttributeVersion(generator.VersionTypeName);
            foreach (var file in files)
            {
                try
                {
                    Stream content = _metadataStore.GetFileStream(file.Id);
                    var thumbnailValue = generator.GenerateVersion(content);
                    _metadataStore.AddVersion(file.Id, generator.VersionTypeName, thumbnailValue);
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
