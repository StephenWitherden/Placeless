using System;
using System.Collections.Generic;
using System.Text;

namespace Placeless.MetadataStore.Sql
{
    public class File
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Extension { get; set; }
        public virtual ICollection<FileSource> FileSources { get; set; }
        public string Hash { get; set; }
        public virtual ICollection<FileAttributeValue> FileAttributeValues { get; set; }
        public virtual ICollection<Version> Versions { get; set; }
    }
}
