using System;
using System.Collections.Generic;
using System.Text;

namespace Placeless.MetadataStore.Sql
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<FileSource> FileSources { get; set; }
    }
}
