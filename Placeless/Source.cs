using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Placeless
{
    public abstract class SourceBase : ISource
    {
        private readonly IMetadataStore _metadataStore;

        public SourceBase(IMetadataStore metadataStore)
        {
            _metadataStore = metadataStore;
        }

        public Task Discover()
        {
            throw new NotImplementedException();
        }

        public abstract string GetName();

        public abstract IEnumerable<string> GetRoots();

        public abstract string GetMetadata(string path);

        public Task RefreshMetadata()
        {
            var roots = GetRoots();

            foreach (var root in roots)
            {
                var _existingSources = _metadataStore.ExistingSources(GetName(), root);
                foreach (var existingSource in _existingSources)
                {
                    string metadata = GetMetadata(existingSource);
                    _metadataStore.UpdateMetadataForSource(existingSource, metadata);
                }
            }
            return Task.CompletedTask;
        }

        public Task Retrieve()
        {
            throw new NotImplementedException();
        }
    }
}
