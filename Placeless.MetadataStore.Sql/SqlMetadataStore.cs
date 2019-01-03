using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Placeless.MetadataStore.Sql
{
    public class SqlMetadataStore : IMetadataStore
    {
        private readonly IPlacelessconfig _configuration;
        private readonly DbContextOptions _dbContextOptions;
        private readonly IUserInteraction _userInteraction;

        const string CONNECTION_STRING_SETTING = "ConnectionStrings:PlacelessDatabase";

        public SqlMetadataStore(IPlacelessconfig configuration, IUserInteraction userInteraction)
        {
            _configuration = configuration;
            _userInteraction = userInteraction;
            if (string.IsNullOrWhiteSpace(_configuration.GetValue(CONNECTION_STRING_SETTING)))
            {
                _userInteraction.ReportError($"Missing expected configuration value: {CONNECTION_STRING_SETTING}");
            }

            _dbContextOptions = SqlServerDbContextOptionsExtensions.UseSqlServer(
                new DbContextOptionsBuilder(),
                _configuration.GetValue(CONNECTION_STRING_SETTING),
                options => options.CommandTimeout(120)
                ).Options;
        }

        static string CalculateMD5(Stream stream)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        public HashSet<string> ExistingSources(string sourceName, string startsWith)
        {
            using (var dbContext = new ApplicationDbContext(_dbContextOptions))
            {
                startsWith += '%';

                var results = dbContext
                    .FileSources.FromSql($"SELECT * FROM FileSources with (nolock) WHERE SourceUri Like {startsWith} and CHARINDEX( '\\', sourceUri, {startsWith.Length + 1}) = 0 ")
                    .Where(f => f.Source.Name == sourceName)
                    .Select(s => s.SourceUri.ToLower());
                return new HashSet<string>(results);
            }
        }

        public async Task AddDiscoveredFile(Stream fileStream, string title, string originalLocation, string metadata, string sourceName)
        {
            using (var dbContext = new ApplicationDbContext(_dbContextOptions))
            {

                var matchingFileSource = await dbContext.FileSources.Where(s => s.SourceUri == originalLocation).AnyAsync();
                if (matchingFileSource)
                {
                    return;
                }

                var source = await dbContext.Sources.Where(s => s.Name.ToLower() == sourceName.ToLower()).FirstOrDefaultAsync();
                if (source == null)
                {
                    source = new Source
                    {
                        Name = sourceName
                    };
                    await dbContext.AddAsync(source);
                    await dbContext.SaveChangesAsync();
                }

                string hash = CalculateMD5(fileStream);
                fileStream.Seek(0, SeekOrigin.Begin);
                var file = await dbContext.Files.Where(f => f.Hash == hash).FirstOrDefaultAsync();

                if (file != null)
                {
                    // a matching file was found, but maybe it's a hash collision
                    if (!StreamsAreEqual(fileStream, new MemoryStream(file.Contents, false)))
                    {
                        // the files are actually different, force recreation of the file
                        file = null;
                    }
                }

                if (file == null)
                {
                    if (fileStream.Length > int.MaxValue)
                    {
                        throw new ArgumentOutOfRangeException("fileStream.Length", fileStream.Length, "File too large");
                    }
                    MemoryStream ms = new MemoryStream((int)fileStream.Length);
                    fileStream.CopyTo(ms);
                    file = new File
                    {
                        Contents = ms.GetBuffer(),
                        Hash = hash,
                        FileGuid = Guid.NewGuid(),
                        Title = title
                    };
                    await dbContext.AddAsync(file);
                    await dbContext.SaveChangesAsync();
                }

                var fileSource = new FileSource
                {
                    FileId = file.Id,
                    Metadata = metadata,
                    SourceUri = originalLocation,
                    SourceId = source.Id
                };
                await dbContext.AddAsync(fileSource);
                await dbContext.SaveChangesAsync();
            }
        }

        private bool StreamsAreEqual(Stream streamA, MemoryStream streamB)
        {
            try
            {
                for (int i = 0; i < streamB.Length; i++)
                {
                    int aByte = streamA.ReadByte();
                    int bByte = streamB.ReadByte();
                    if (aByte.CompareTo(bByte) != 0)
                        return false;
                }
                return true;
            }
            finally
            {
                streamA.Seek(0, SeekOrigin.Begin);
                streamB.Seek(0, SeekOrigin.Begin);
            }
        }

        public void RefreshMetadata()
        {
            throw new NotImplementedException();
        }

        public async Task UpdateMetadataForSource(string existingSource, string metadata)
        {
            using (var dbContext = new ApplicationDbContext(_dbContextOptions))
            {
                var sources = dbContext.FileSources.Where(e => e.SourceUri == existingSource);
                foreach (var fileSource in sources)
                {
                    fileSource.Metadata = metadata;
                }
                await dbContext.SaveChangesAsync();
            }
        }

        public IList<Placeless.File> FilesMissingAttribute(string attributeName)
        {
            using (var dbContext = new ApplicationDbContext(_dbContextOptions))
            {
                var attribute = dbContext.Attributes.Where(a => a.Name == attributeName).FirstOrDefault();
                if (attribute == null)
                {
                    attribute = new Attribute { Name = attributeName };
                    dbContext.Add(attribute);
                    dbContext.SaveChanges();
                }

                return dbContext.Files
                    .Where(f => !f.FileAttributeValues.Any(v => v.AttributeValue.AttributeId == attribute.Id))
                    .Select(f => new Placeless.File { Id = f.Id, Metadata = f.FileSources.Select(s => s.Metadata) })
                    .ToList();
            }
        }

        public void SetAttribute(int id, string attributeName, string value)
        {
            using (var dbContext = new ApplicationDbContext(_dbContextOptions))
            {
                var attribute = dbContext.Attributes.Where(a => a.Name == attributeName).FirstOrDefault();
                if (attribute == null)
                {
                    attribute = new Attribute { Name = attributeName };
                    dbContext.Add(attribute);
                    dbContext.SaveChanges();
                }

                var attributeValue = dbContext.AttributeValues.Where(a => a.AttributeId == attribute.Id & a.Value == value).FirstOrDefault();
                if (attributeValue == null)
                {
                    attributeValue = new AttributeValue { AttributeId = attribute.Id, Value = value };
                    dbContext.Add(attributeValue);
                    dbContext.SaveChanges();
                }

                var fileAttributeValue = dbContext.FileAttributeValues
                    .Where(a => a.FileId == id && a.AttributeValueId == attributeValue.Id).FirstOrDefault();
                if (fileAttributeValue == null)
                {
                    fileAttributeValue = new FileAttributeValue { FileId = id, AttributeValueId = attributeValue.Id };
                    dbContext.Add(fileAttributeValue);
                    dbContext.SaveChanges();
                }
            }
        }

        public IList<Placeless.File> FilesMissingAttributeVersion(string versionTypeName)
        {
            using (var dbContext = new ApplicationDbContext(_dbContextOptions))
            {
                return dbContext.Files
                    .Where(f => !(f.Versions.Any(v => v.VersionType.Name == versionTypeName)))
                    .Select(f => new Placeless.File { Id = f.Id })
                    .ToList();
            }
        }

        public Stream GetFileStream(int id)
        {
            using (var dbContext = new ApplicationDbContext(_dbContextOptions))
            {
                var file = dbContext.Files.Where(f => f.Id == id).FirstOrDefault();
                return new MemoryStream(file.Contents);
            }
        }

        public void AddVersion(int fileId, string versionTypeName, string thumbnailString)
        {
            using (var dbContext = new ApplicationDbContext(_dbContextOptions))
            {
                var versionType = dbContext.VersionTypes.Where(vt => vt.Name == versionTypeName).FirstOrDefault();
                if (versionType == null)
                {
                    versionType = new VersionType
                    {
                        Name = versionTypeName
                    };
                    dbContext.VersionTypes.Add(versionType);
                    dbContext.SaveChanges();
                }

                var version = dbContext.Versions
                    .Where(v => v.FileId == fileId && v.VersionTypeId == versionType.Id)
                    .FirstOrDefault();


                if (version == null)
                {
                    version = new Version
                    {
                        FileId = fileId,
                        VersionTypeId = versionType.Id,
                        Contents = thumbnailString
                    };
                    dbContext.Versions.Add(version);
                    dbContext.SaveChanges();
                }
            }
        }

        public IList<string> AllAttributeValues(string attributeName)
        {
            using (var dbContext = new ApplicationDbContext(_dbContextOptions))
            {
                return dbContext.AttributeValues.Where(a => a.Attribute.Name == attributeName)
                    .Select(a => a.Value)
                    .OrderBy(v => v)
                    .ToList();
            }
        }

        public IList<Thumbnail> ThumbnailsForAttributeValue(string attributeName, string attributeValue)
        {
            using (var dbContext = new ApplicationDbContext(_dbContextOptions))
            {
                return dbContext.Versions.Where
                    (v => v.VersionType.Name == "Medium Thumbnail" && 
                        v.File.FileAttributeValues.Any
                            (f => f.AttributeValue.Value == attributeValue && 
                            f.AttributeValue.Attribute.Name == attributeName))
                    .Select(a => new Thumbnail { Content = a.Contents, Fileid = a.FileId }).ToList();
            }
        }

    }
}
