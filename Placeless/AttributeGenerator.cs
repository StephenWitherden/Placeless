using System;
using System.Collections.Generic;
using System.Text;

namespace Placeless
{
    public abstract class AttributeGenerator
    {
        public abstract string AttributeName { get; }
        public abstract string GenerateAttribute(File file);
    }
}
