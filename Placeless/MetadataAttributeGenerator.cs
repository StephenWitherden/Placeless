using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Placeless
{
    public abstract class MetadataAttributeGenerator : AttributeGenerator
    {
        public override string[] GenerateAttribute(File file)
        {
            var values = file.Metadata.Select(DeriveFromMetadata)
                .SelectMany(s => s.ToArray())
                .Distinct();
            return values.ToArray();
        }

        public abstract string[] DeriveFromMetadata(string metadata);

    }
}
