using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Placeless.Configuration.AspDotNet
{
    public class AspDotNetConfiguration : IPlacelessconfig
    {
        private readonly IConfigurationRoot _configuration;

        public AspDotNetConfiguration(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        public string GetValue(string path)
        {
            return _configuration.GetValue<String>(path);
        }

        public IEnumerable<string> GetValues(string path)
        {
            return _configuration.GetSection(path).AsEnumerable().Select(s => s.Value);
        }
    }
}
