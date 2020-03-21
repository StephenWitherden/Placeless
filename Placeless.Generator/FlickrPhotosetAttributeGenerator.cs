using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Placeless.Generator
{
    public class FlickrPhotosetAttributeGenerator : MetadataAttributeGenerator
    {
        public override string AttributeName => "Set";

        public override string[] DeriveFromMetadata(string metadata)
        {
            var dateObj = JObject.Parse(metadata)["Sets"];
            if (dateObj != null)
            {
                var jValues = dateObj.Values();
                var values = new List<string>();
                foreach(var value in jValues)
                {
                    values.Add(value.ToString());
                }
                return values.ToArray();
            }
            return new string[] { };
        }
    }
}
