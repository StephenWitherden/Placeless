using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;

namespace Placeless
{
    public class Collector 
    {
        private readonly IMetadataStore _metadataStore;
        private readonly ISource _source;
        private readonly IUserInteraction _userInteraction;
        private ConcurrentQueue<string> _roots;
        private ConcurrentQueue<DiscoveredFile> _files;

        protected int _discoveredRoots = 0;
        protected int _processedRoots = 0;
        protected int _discoveredFiles = 0;
        protected int _processedFiles = 0;

        public int DiscoveredRoots
        {
            get
            {
                return _discoveredRoots;
            }
        }

        public int ProcessedRoots
        {
            get
            {
                return _processedRoots;
            }
        }


        public int DiscoveredFiles
        {
            get
            {
                return _discoveredFiles;
            }
        }

        public int ProcessedFiles
        {
            get
            {
                return _processedFiles;
            }
        }

        public Collector(IMetadataStore metadataStore, ISource source, IUserInteraction userInteraction)
        {
            _metadataStore = metadataStore;
            _source = source;
            _roots = new ConcurrentQueue<string>();
            _files = new ConcurrentQueue<DiscoveredFile>();
            _userInteraction = userInteraction;
        }

        public async Task Discover()
        {
            var options = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 10000,
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                SingleProducerConstrained = true
            };

            var rootBuffer = new BufferBlock<string>();
            var fileBuffer = new BufferBlock<DiscoveredFile>();

            var countFileBlock = new TransformBlock<DiscoveredFile, DiscoveredFile>(file =>
            {
                Interlocked.Increment(ref _discoveredFiles);
                return file;
            });

            var countRootsBlock = new TransformBlock<string, string>(root =>
            {
                Interlocked.Increment(ref _discoveredRoots);
                return root;
            });

            var discoverFilesBlock = new TransformManyBlock<string, DiscoveredFile>(root =>
            {
                try
                {
                    var existingSources = _metadataStore.ExistingSources(_source.GetName(), root);
                    return _source.Discover(root, existingSources);
                }
                catch (Exception ex)
                {
                    _userInteraction.ReportError(ex.Message);
                    return new DiscoveredFile[] { };
                }
                finally
                {
                    Interlocked.Increment(ref _processedRoots);
                }
            }, options);

            var injestFileBlock = new ActionBlock<DiscoveredFile>(async file =>
            {
                try
                {
                    var stream = _source.GetContents(file.Url);
                    string metadata = await _source.GetMetadata(file.Path);
                    await _metadataStore.AddDiscoveredFile(stream, file.Name, file.Path, metadata, _source.GetName());
                }
                catch (Exception ex)
                {
                    _userInteraction.ReportError(ex.Message);
                }
                finally
                {
                    Interlocked.Increment(ref _processedFiles);
                }
            }, options);

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            countRootsBlock.LinkTo(rootBuffer, linkOptions);
            rootBuffer.LinkTo(discoverFilesBlock, linkOptions);
            discoverFilesBlock.LinkTo(countFileBlock, linkOptions);
            countFileBlock.LinkTo(fileBuffer, linkOptions);
            fileBuffer.LinkTo(injestFileBlock, linkOptions);


            Task getRoots = new Task(async () =>
            {
                var roots = _source.GetRoots();
                foreach (var root in roots)
                {
                    await countRootsBlock.SendAsync(root);
                }
                countRootsBlock.Complete();
            }, TaskCreationOptions.LongRunning );
            getRoots.Start();

            await injestFileBlock.Completion;

        }

        public async Task RefreshMetadata()
        {
            var options = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 10000,
                MaxDegreeOfParallelism = Environment.ProcessorCount, 
                SingleProducerConstrained = true
            };

            var rootBuffer = new BufferBlock<string>();
            var fileBuffer = new BufferBlock<string>();

            var countRootsBlock = new TransformBlock<string, string>(root =>
            {
                Interlocked.Increment(ref _discoveredRoots);
                return root;
            });

            var countFileBlock = new TransformBlock<string, string>(file =>
            {
                Interlocked.Increment(ref _discoveredFiles);
                return file;
            });

            var discoverFilesBlock = new TransformManyBlock<string, string>(root =>
            {
                try
                {
                    return _metadataStore.ExistingSources(_source.GetName(), root);
                }
                catch (Exception ex)
                {
                    _userInteraction.ReportError(ex.Message);
                    return new string[] { };
                }
                finally
                {
                    Interlocked.Increment(ref _processedRoots);
                }
            }, options);

            var injestFileBlock = new ActionBlock<string>(async file =>
            {
                try
                {
                    string metadata = await _source.GetMetadata(file);
                    await _metadataStore.UpdateMetadataForSource(file, metadata);
                }
                catch (Exception ex)
                {
                    _userInteraction.ReportError(ex.Message);
                }
                finally
                {
                    Interlocked.Increment(ref _processedFiles);
                }
            }, options);

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            countRootsBlock.LinkTo(rootBuffer, linkOptions);
            rootBuffer.LinkTo(discoverFilesBlock, linkOptions);
            discoverFilesBlock.LinkTo(countFileBlock, linkOptions);
            countFileBlock.LinkTo(fileBuffer, linkOptions);
            fileBuffer.LinkTo(injestFileBlock, linkOptions);

            Task getRoots = new Task(async () =>
            {
                var roots = _source.GetRoots();
                foreach (var root in roots)
                {
                    await countRootsBlock.SendAsync(root);
                }
                countRootsBlock.Complete();
            }, TaskCreationOptions.LongRunning);
            getRoots.Start();

            await injestFileBlock.Completion;
        }
    }
}
