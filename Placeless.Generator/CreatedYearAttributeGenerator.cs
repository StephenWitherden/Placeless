using Newtonsoft.Json.Linq;
using System;

namespace Placeless.Generator
{
    public class CreatedYearAttributeGenerator : MetadataAttributeGenerator
    {
        public override string AttributeName => "Year Created";

        public override string DeriveFromMetadata(string metadata)
        {
            var dateObj = JObject.Parse(metadata)["File_File_Modified_Date"];
            if (dateObj != null)
            {
                var date = dateObj.Value<string>();
                string year = date.Substring(date.Length - 4, 4);
                return year;
            }
            dateObj = JObject.Parse(metadata)["DateTaken"];
            if (dateObj != null)
            {
                var date = dateObj.Value<DateTime>();
                string year = date.Year.ToString();
                return year;
            }
            return null;
        }
    }
}
