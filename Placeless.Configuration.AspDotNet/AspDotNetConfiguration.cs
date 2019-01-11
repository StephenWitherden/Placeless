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
            return _configuration[path];
        }

        public IEnumerable<string> GetValues(string path)
        {
            return _configuration.GetSection(path).AsEnumerable().Select(s => s.Value);
        }

        public void Save()
        {
            
        }

        public void SetValue(string path, string value)
        {
            _configuration[path] = value;
        }

        public void SetValues(string path, IEnumerable<string> values)
        {
            int i = 0;
            foreach (var value in values)
            {
                _configuration[$"{path}:{i}"] = value;
                i++;
            }

        }
    }
}
