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
        IEnumerable<File> FilesMissingAttribute(string attributeName);
        int CountFilesMissingAttribute(string attributeName);
        IEnumerable<Placeless.AttributeValue> AllAttributeValues(string metadataType);
        Task SetAttribute(int id, string attributeName, string value);
        IEnumerable<File> FilesMissingAttributeVersion(string versionTypeName);
        int CountFilesMissingAttributeVersion(string versionTypeName);
        Stream GetFileStream(int id);
        Task AddVersion(int id, string versionTypeName, string thumbnailStream);
        IList<Thumbnail> ThumbnailsForAttributeValue(string attributeName, string attributeValue);
        IList<Thumbnail> ThumbnailsForAttributeValue(int id);
    }
}
