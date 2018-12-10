using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Placeless.MetadataStore.Sql
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<File> Files { get; set; }
        public DbSet<Source> Sources { get; set; }
        public DbSet<FileSource> FileSources { get; set; }
        public DbSet<Attribute> Attributes { get; set; }
        public DbSet<AttributeValue> AttributeValues { get; set; }
        public DbSet<FileAttributeValue> FileAttributeValues { get; set; }
        public DbSet<Version> Versions { get; set; }
        public DbSet<VersionType> VersionTypes { get; set; }
    }
}
