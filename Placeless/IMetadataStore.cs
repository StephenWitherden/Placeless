using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Placeless
{
    public interface IMetadataStore
    {
        Task AddDiscoveredFile(Stream fileStream, string Title, string originalLocation, string metadata, string sourceName);
        void RefreshMetadata();
        HashSet<string> ExistingSources(string sourceName, string startswith);
        Task UpdateMetadataForSource(string existingSource, string metadata);
        IList<File> FilesMissingAttribute(string attributeName);
        IList<string> AllAttributeValues(string metadataType);
        void SetAttribute(int id, string attributeName, string value);
        IList<File> FilesMissingAttributeVersion(string versionTypeName);
        Stream GetFileStream(int id);
        void AddVersion(int id, string versionTypeName, string thumbnailStream);
        IList<Thumbnail> ThumbnailsForAttributeValue(string attributeName, string attributeValue);
    }
}
