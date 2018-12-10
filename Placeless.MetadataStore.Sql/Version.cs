using System;
using System.Collections.Generic;
using System.Text;

namespace Placeless.MetadataStore.Sql
{
    public class Version
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public virtual File File { get; set; }
        public string Contents { get; set; }
        public int VersionTypeId { get; set; }
        public VersionType VersionType { get; set; }
    }
}
