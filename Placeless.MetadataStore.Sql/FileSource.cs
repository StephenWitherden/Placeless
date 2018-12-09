using System;
using System.Collections.Generic;
using System.Text;

namespace Placeless.MetadataStore.Sql
{
    public class FileSource
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public virtual File File { get; set; }
        public int SourceId { get; set; }
        public virtual Source Source { get; set; }
        public string SourceUri { get; set; }
        public string Metadata { get; set; }
    }
}
