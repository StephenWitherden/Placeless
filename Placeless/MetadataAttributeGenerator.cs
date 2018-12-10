using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Placeless
{
    public abstract class MetadataAttributeGenerator : AttributeGenerator
    {
        public override string GenerateAttribute(File file)
        {
            var values = file.Metadata.Select(DeriveFromMetadata).Distinct();
            if (values.Count() == 1)
            {
                return values.First();
            }
            else
            {
                // TODO
                return values.Min();
            }
        }

        public abstract string DeriveFromMetadata(string metadata);

    }
}
