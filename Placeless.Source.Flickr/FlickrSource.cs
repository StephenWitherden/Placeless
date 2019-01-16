using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class FlickrSource : ISource
    {
        public static string TOKEN_PATH = "Flickr:Token";
        public static string SECRET_PATH = "Flickr:Secret";

        const string API_KEY = "c8cd6dd8f93f95b681a5e1ea321b6c5f";
        const string API_SECRET = "88ee851e6d54b867";

        private readonly IMetadataStore _metadataStore;
        private readonly IPlacelessconfig _configuration;
        private readonly FlickrNet.Flickr _flickr;
        private readonly IUserInteraction _userInteraction;

        private class DownloadState
        {
            public string PhotosetId { get; set; }
            public string PhotoId { get; set; }
            public string Title { get; set; }
            public string Url { get; set; }
        }

        public FlickrSource(IMetadataStore store, IPlacelessconfig configuration, IUserInteraction userInteraction)
        {
            _metadataStore = store;
            _configuration = configuration;
            _userInteraction = userInteraction;
            _flickr = new FlickrNet.Flickr(API_KEY, API_SECRET);
            _flickr.InstanceCacheDisabled = true;
            var token = _configuration.GetValue(TOKEN_PATH);
            var secret = _configuration.GetValue(SECRET_PATH);

            token = "";
            secret = "";

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(secret))
            {
                var requestToken = _flickr.OAuthGetRequestToken("oob");
                string url = _flickr.OAuthCalculateAuthorizationUrl(requestToken.Token, AuthLevel.Read);
                _userInteraction.OpenWebPage(url);
                string approvalCode = _userInteraction.InputPrompt("Please approve access to your Flickr account and enter the key here:");

                var accessToken = _flickr.OAuthGetAccessToken(requestToken, approvalCode);
                token = accessToken.Token;
                secret = accessToken.TokenSecret;
                _configuration.SetValue(TOKEN_PATH, token);
                _configuration.SetValue(SECRET_PATH, secret);
            }
            _flickr.OAuthAccessToken = token;
            _flickr.OAuthAccessTokenSecret = secret;

        }

        public IEnumerable<string> GetRoots()
        {
            yield return "/"; // for photos not in a set
            foreach (var root in _flickr.PhotosetsGetList().Select(s => s.PhotosetId + "/"))
            {
                yield return root;
            }
        }

        public Stream GetContents(string url)
        {
            using (WebClient wc = new WebClient())
            {
                var data = wc.DownloadData(url);
                var stream = new MemoryStream(data);
                return stream;
            }
        }

        public IEnumerable<DiscoveredFile> Discover(string path, HashSet<string> existingSources)
        {
            string photosetId = path.Replace("/", "");
            int page = 1;
            int maxPages = 1;

            while (page <= maxPages)
            {
                // existing sources gives us a list of photoset ids
                PagedPhotoCollection photos;
                try
                {
                    if (photosetId == "")
                    {
                        photos = _flickr.PhotosGetNotInSet(page, 100, PhotoSearchExtras.OriginalUrl | PhotoSearchExtras.Media | PhotoSearchExtras.LargeUrl);
                    }
                    else
                    {
                        photos = _flickr.PhotosetsGetPhotos(photosetId, PhotoSearchExtras.OriginalUrl | PhotoSearchExtras.Media | PhotoSearchExtras.LargeUrl, page, 100);
                    }
                    maxPages = photos.Pages;
                    page++;
                }
                catch
                {
                    photos = new PhotoCollection();
                }

                foreach (var photo in photos)
                {
                    if (!existingSources.Contains(path + photo.PhotoId))
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

                        yield return
                            new DiscoveredFile
                            {
                                Name = photo.Title,
                                Path = path + photo.PhotoId,
                                Extension = System.IO.Path.GetExtension(url),
                                Url = url
                            };
                    }
                }
            }
        }


        public async Task<string> GetMetadata(string path)
        {
            string photoId = path.Split('/')[1];

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

            return await Task.FromResult(j.ToString());

        }

        private string Clean(string name)
        {
            return name.Replace(' ', '_').Replace('/', '_').Replace('-', '_');
        }

        public string GetName()
        {
            return "Flickr";
        }

    }
}
