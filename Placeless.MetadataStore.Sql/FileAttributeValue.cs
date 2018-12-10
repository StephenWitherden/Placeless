using System;
using System.Collections.Generic;
using System.Text;

namespace Placeless.MetadataStore.Sql
{
    public class FileAttributeValue
    {
        public int Id { get; set; }
        public int AttributeValueId { get; set; }
        public AttributeValue AttributeValue { get; set; }
        public int FileId { get; set; }
        public File File { get; set; }
        public int AssignmentType { get; set; }
    }
}
