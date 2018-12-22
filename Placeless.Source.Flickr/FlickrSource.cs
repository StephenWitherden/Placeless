using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FlickrNet;
using Newtonsoft.Json.Linq;

namespace Placeless.Source.Flickr
{
    public class FlickrSource : SourceBase
    {
        private readonly IMetadataStore _metadataStore;
        private readonly IPlacelessconfig _configuration;
        private HashSet<string> _existingSources;
        private readonly FlickrNet.Flickr _flickr;
        private readonly IUserInteraction _userInteraction;
        const int MAX_THREADS = 20;
        SemaphoreSlim maxThread = new SemaphoreSlim(MAX_THREADS);

        private class DownloadState
        {
            public string PhotosetId { get; set; }
            public string PhotoId { get; set; }
            public string Title { get; set; }
            public string Url { get; set; }
        }

        public FlickrSource(IMetadataStore store, IPlacelessconfig configuration, IUserInteraction userInteraction)
            : base(store)
        {
            _metadataStore = store;
            _configuration = configuration;
            _flickr = new FlickrNet.Flickr(_configuration.GetValue("Flickr:ApiKey"), _configuration.GetValue("Flickr:SharedSecret"));
            _userInteraction = userInteraction;

            var requestToken = _flickr.OAuthGetRequestToken("oob");

            string url = _flickr.OAuthCalculateAuthorizationUrl(requestToken.Token, AuthLevel.Read);
            _userInteraction.OpenWebPage(url);
            string key = _userInteraction.InputPrompt("Please approve access to your Flickr account and enter the key here:");

            var accessToken = _flickr.OAuthGetAccessToken(requestToken, key);
            _flickr.OAuthAccessToken = accessToken.Token;
        }

        public override IEnumerable<string> GetRoots()
        {
            var photoSets = _flickr.PhotosetsGetList().Select(s => s.PhotosetId + "/")
                .ToList();
            photoSets.Append("/"); // for photos that don't belong to a set
            return photoSets;
        }

        public Task Discover()
        {
            var done = false;
            while (!done)
            {
                try
                {
                    var photoSets = _flickr.PhotosetsGetList();

                    foreach (var photoSet in photoSets)
                    {
                        _existingSources = _metadataStore.ExistingSources(GetName(), photoSet.PhotosetId + "/");
                        var photosInSet = _flickr.PhotosetsGetPhotos(photoSet.PhotosetId, PhotoSearchExtras.OriginalUrl | PhotoSearchExtras.Media | PhotoSearchExtras.LargeUrl);
                        Discover(_existingSources, photosInSet, photoSet.PhotosetId);
                    }
                    var photos = _flickr.PhotosGetNotInSet();
                    _existingSources = _metadataStore.ExistingSources(GetName(), "/");
                    Discover(_existingSources, photos, "");

                    while(maxThread.CurrentCount < MAX_THREADS)
                    {
                        Thread.Sleep(1000);
                    }

                    done = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    done = false;
                }
            }
            return Task.CompletedTask;
        }

        private void Discover(HashSet<string> existingSources, PagedPhotoCollection photos, string photoSetId)
        {
            foreach (var photo in photos)
            {
                if (!_existingSources.Contains(photoSetId + "/" + photo.PhotoId))
                {
                    if (photo.MediaStatus == "failed")
                    {
                        continue;
                    }
                    string url = photo.OriginalUrl;
                    if (url == null)
                    {
                        url = photo.LargeUrl;
                    }
                    if (url == null)
                    {
                        url = photo.MediumUrl;
                    }

                    maxThread.Wait();
                    ThreadPool.QueueUserWorkItem(DownloadPicture, new DownloadState { PhotoId = photo.PhotoId, PhotosetId = photoSetId, Title = photo.Title, Url = url });
                }
            }
        }

        private void DownloadPicture(Object stateInfo)
        {
            var downloadinfo = stateInfo as DownloadState;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    var data = wc.DownloadData(downloadinfo.Url);
                    var stream = new MemoryStream(data);
                    string metadata = GetMetadata(downloadinfo.PhotoId);
                    _metadataStore.AddDiscoveredFile(stream, downloadinfo.Title, downloadinfo.PhotosetId + "/" + downloadinfo.PhotoId, metadata, GetName());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            maxThread.Release();
        }

        public override string GetMetadata(string photoId)
        {
            var info = _flickr.PhotosGetInfo(photoId);
            var exif = _flickr.PhotosGetExif(photoId);
            var contexts = _flickr.PhotosGetAllContexts(photoId);

            JObject j = new JObject();
            j.Add("DateLastUpdated", info.DateLastUpdated);
            j.Add("DatePosted", info.DatePosted);
            j.Add("DateTaken", info.DateTaken);
            j.Add("DateUploaded", info.DateUploaded);
            j.Add("Description", info.Description);
            j.Add("HasPeople", info.HasPeople);
            j.Add("IsPublic", info.IsPublic);
            j.Add("License", info.License.ToString());
            j.Add("Latitude", info.Location?.Latitude);
            j.Add("Longitude", info.Location?.Longitude);
            j.Add("Title", info.Title);
            j.Add("IsFamily", info.IsFamily);
            j.Add("IsFavorite", info.IsFavorite);
            j.Add("IsFriend", info.IsFriend);
            j.Add("Rotation", info.Rotation);
            j.Add("WebUrl", info.WebUrl);
            j.Add("Media", Enum.GetName(typeof(MediaType), info.Media));
            j.Add("SafetyLevel", Enum.GetName(typeof(SafetyLevel), info.SafetyLevel));
            j.Add("Tags", new JArray(info.Tags.Select(t => t.TagText).ToArray()));
            j.Add("Urls", new JArray(info.Urls.Select(t => t.Url).ToArray()));
            j.Add("Notes", new JArray(info.Notes.Select(t => t.NoteText).ToArray()));
            j.Add("ViewCount", info.ViewCount);

            j.Add("Groups", new JArray(contexts.Groups.Select(t => t.Title).ToArray()));
            j.Add("Sets", new JArray(contexts.Sets.Select(t => t.Title).ToArray()));


            exif.ToList().ForEach(e =>
            {
                j.Add(e.TagSpace + "." + e.Tag, e.CleanOrRaw);
            });

            return j.ToString();

        }

        private string Clean(string name)
        {
            return name.Replace(' ', '_').Replace('/', '_').Replace('-', '_');
        }

        public override string GetName()
        {
            return "Flickr";
        }
    }
}
