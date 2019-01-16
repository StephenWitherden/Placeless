using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
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
        private readonly IBlobStore _blobStore;

        public const string CONNECTION_STRING_SETTING = "ConnectionStrings:PlacelessDatabase";

        public SqlMetadataStore(IPlacelessconfig configuration, IUserInteraction userInteraction, IBlobStore blobStore)
        {
            _configuration = configuration;
            _userInteraction = userInteraction;
            _blobStore = blobStore;

            if (string.IsNullOrWhiteSpace(_configuration.GetValue(CONNECTION_STRING_SETTING)))
            {
                _userInteraction.ReportError($"Missing expected configuration value: {CONNECTION_STRING_SETTING}");
            }

            _dbContextOptions = SqlServerDbContextOptionsExtensions.UseSqlServer(
                new DbContextOptionsBuilder(),
                _configuration.GetValue(CONNECTION_STRING_SETTING),
                options => options.CommandTimeout(120)
                ).Options;

            using (var dbContext = new ApplicationDbContext(_dbContextOptions))
            {
                dbContext.Database.Migrate();
            }
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

        public async Task AddDiscoveredFile(Stream fileStream, string title, string extension, string originalLocation, string metadata, string sourceName)
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
                    if (!StreamsAreEqual(fileStream, _blobStore.Get(file.Id)))
                    {
                        // the files are actually different, force recreation of the file
                        file = null;
                    }
                }

                if (file == null)
                {
                    file = new File
                    {
                        Hash = hash,
                        Title = title,
                        Extension = extension
                    };
                    await dbContext.AddAsync(file);
                    await dbContext.SaveChangesAsync();
                    await _blobStore.PutAsync(fileStream, file.Id);
                }

                var fileSource = new FileSource
                {
                    FileId = file.Id,
                    Metadata = metadata,
                    SourceUri = originalLocation,
                    SourceId = source.Id,
                };
                await dbContext.AddAsync(fileSource);
                await dbContext.SaveChangesAsync();
            }
        }

        private bool StreamsAreEqual(Stream streamA, Stream streamB)
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


        public int CountFilesMissingAttribute(string attributeName)
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
                    .Count();
            }
        }

        public IEnumerable<Placeless.File> FilesMissingAttribute(string attributeName)
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

                var files = dbContext.Files
                    .Where(f => !f.FileAttributeValues.Any(v => v.AttributeValue.AttributeId == attribute.Id))
                    .Select(f => new Placeless.File { Id = f.Id, Metadata = f.FileSources.Select(s => s.Metadata) });

                foreach (var file in files)
                {
                    yield return file;
                }

            }
        }

        public async Task SetAttribute(int id, string attributeName, string value)
        {
            using (var dbContext = new ApplicationDbContext(_dbContextOptions))
            {
                var attribute = await dbContext.Attributes.Where(a => a.Name == attributeName).FirstOrDefaultAsync();
                if (attribute == null)
                {
                    attribute = new Attribute { Name = attributeName };
                    await dbContext.AddAsync(attribute);
                    await dbContext.SaveChangesAsync();
                }

                var attributeValue = await dbContext.AttributeValues.Where(a => a.AttributeId == attribute.Id & a.Value == value).FirstOrDefaultAsync();
                if (attributeValue == null)
                {
                    attributeValue = new AttributeValue { AttributeId = attribute.Id, Value = value };
                    await dbContext.AddAsync(attributeValue);
                    await dbContext.SaveChangesAsync();
                }

                var fileAttributeValue = await dbContext.FileAttributeValues
                    .Where(a => a.FileId == id && a.AttributeValueId == attributeValue.Id).FirstOrDefaultAsync();
                if (fileAttributeValue == null)
                {
                    fileAttributeValue = new FileAttributeValue { FileId = id, AttributeValueId = attributeValue.Id };
                    await dbContext.AddAsync(fileAttributeValue);
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        public IEnumerable<Placeless.File> FilesMissingAttributeVersion(string versionTypeName)
        {
            using (var dbContext = new ApplicationDbContext(_dbContextOptions))
            {
                var files = dbContext.Files
                    .Where(f => !(f.Versions.Any(v => v.VersionType.Name == versionTypeName)))
                    .Select(f => new Placeless.File { Id = f.Id }); 

                foreach (var file in files)
                {
                    yield return file;
                }
            }
        }

        public int CountFilesMissingAttributeVersion(string versionTypeName)
        {
            using (var dbContext = new ApplicationDbContext(_dbContextOptions))
            {
                return dbContext.Files
                    .Where(f => !(f.Versions.Any(v => v.VersionType.Name == versionTypeName)))
                    .Count();
            }
        }

        public Stream GetFileStream(int id)
        {
            return _blobStore.Get(id);
        }

        public async Task AddVersion(int fileId, string versionTypeName, string thumbnailString)
        {
            using (var dbContext = new ApplicationDbContext(_dbContextOptions))
            {
                var versionType = await dbContext.VersionTypes.Where(vt => vt.Name == versionTypeName).FirstOrDefaultAsync();
                if (versionType == null)
                {
                    versionType = new VersionType
                    {
                        Name = versionTypeName
                    };
                    await dbContext.VersionTypes.AddAsync(versionType);
                    await dbContext.SaveChangesAsync();
                }

                var version = await dbContext.Versions
                    .Where(v => v.FileId == fileId && v.VersionTypeId == versionType.Id)
                    .FirstOrDefaultAsync();


                if (version == null)
                {
                    version = new Version
                    {
                        FileId = fileId,
                        VersionTypeId = versionType.Id,
                        Contents = thumbnailString
                    };
                    await dbContext.Versions.AddAsync(version);
                    await dbContext.SaveChangesAsync();
                }
            }
        }


        public IEnumerable<string> AllAttributes()
        {
            using (var dbContext = new ApplicationDbContext(_dbContextOptions))
            {
                return dbContext.Attributes
                    .OrderBy(a => a.Name)
                    .Select(a => a.Name)
                    .ToList();
            }
        }

        public IEnumerable<Placeless.AttributeValue> AllAttributeValues(string attributeName)
        {
            using (var dbContext = new ApplicationDbContext(_dbContextOptions))
            {
                return dbContext.AttributeValues.Where(a => a.Attribute.Name == attributeName)
                    .OrderBy(a => a.Value)
                    .Select(a => new Placeless.AttributeValue { AttributeId = a.AttributeId, Id = a.Id, Value = a.Value }  )
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
                    .Select(a => new Thumbnail { Content = a.Contents, Fileid = a.FileId, Title = a.File.Title }).ToList();
            }
        }

        public IList<Thumbnail> ThumbnailsForAttributeValue(int attributeValueId)
        {
            using (var dbContext = new ApplicationDbContext(_dbContextOptions))
            {
                return dbContext.Versions.Where
                    (v => v.VersionType.Name == "Medium Thumbnail" &&
                        v.File.FileAttributeValues.Any
                            (f => f.AttributeValueId == attributeValueId))
                    .Select(a => new Thumbnail { Content = a.Contents, Fileid = a.FileId, Title = a.File.Title }).ToList();
            }
        }


        public IList<Placeless.File> FilesForAttributeValue(int attributeValueId)
        {
            using (var dbContext = new ApplicationDbContext(_dbContextOptions))
            {
                return dbContext.Files.Where
                    (f => f.FileAttributeValues.Any(f2 => f2.AttributeValueId == attributeValueId))
                    .Select(f => new Placeless.File { Id = f.Id, Extension = f.Extension, Title = f.Title }).ToList();
            }
        }


        public static string GetSqlPath(string connectionString)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            builder.InitialCatalog = "Master";

            using (SqlConnection con = new SqlConnection(builder.ConnectionString))
            {
                con.Open();
                string sql = "select top 1 physical_name FROM sys.master_files WHERE name = N'master'";
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    string path = cmd.ExecuteScalar().ToString();
                    return Path.GetDirectoryName(path);
                }
            }
        }

        public IEnumerable<string> GetMetadata(int id)
        {
            using (var dbContext = new ApplicationDbContext(_dbContextOptions))
            {
                return dbContext.FileSources
                    .Where(s => s.FileId == id)
                    .Select(s => s.Metadata)
                    .ToList();
            }
        }



        const string SQL_CREATE_DATABASE = @"CREATE DATABASE [{0}]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'{0}', FILENAME = N'{1}' , SIZE = 65536KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB ) 
 LOG ON 
( NAME = N'{0}_Log', FILENAME = N'{2}' , SIZE = 65536KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )";

        public static void CreateDatabase(string connectionString, string databaseName, string mdf, string ldf)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            builder.InitialCatalog = "Master";

            using (SqlConnection con = new SqlConnection(builder.ConnectionString))
            {
                con.Open();
                string sql = string.Format(SQL_CREATE_DATABASE, databaseName, mdf, ldf);
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.CommandTimeout = 999;

                    cmd.ExecuteNonQuery();

                }
            }
        }
    }
}
