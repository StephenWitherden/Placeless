using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenTl.ClientApi;
using OpenTl.Schema;
using OpenTl.Schema.Messages;
using static TdLib.TdApi;
using TdLib;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;

namespace Placeless.Source.Telegram
{
    public class SearchChatMessages : TdLib.TdApi.SearchChatMessages, IRequest<Messages>, IObject
    {

    }

    public class TelegramSource : ISource
    {
        private readonly IMetadataStore _metadataStore;
        private readonly IPlacelessconfig _configuration;
        private HashSet<string> _existingSources;
        private readonly IUserInteraction _userInteraction;
        private IClientApi _clientApi;

        public TelegramSource(IMetadataStore store, IPlacelessconfig configuration, IUserInteraction userInteraction)
        {
            _metadataStore = store;
            _configuration = configuration;
            _userInteraction = userInteraction;
        }

        private async Task Discover(int id)
        {
            var searchRequest = new SearchChatMessages
            {
                ChatId = id,
                Filter = new SearchMessagesFilter.SearchMessagesFilterPhotoAndVideo(),                
            };
            searchRequest.ChatId = id;
            

            //var messages = await _clientApi.MessagesService.CustomRequestsService.SendRequestAsync(searchRequest);

        }

        public string GetName()
        {
            return "Telegram";
        }

        public Task Retrieve()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetRoots()
        {
            bool finished = false;
            ConcurrentQueue<string> channelIds = new ConcurrentQueue<string>();

            var task = new Task(async () =>
            {
                var settings = new FactorySettings
                {
                    AppHash = _configuration.GetValue("Telegram:AppHash"),
                    AppId = int.Parse(_configuration.GetValue("Telegram:AppId")),
                    ServerAddress = _configuration.GetValue("Telegram:ServerAddress"),
                    ServerPublicKey = _configuration.GetValue("Telegram:PublicKey"),
                    ServerPort = int.Parse(_configuration.GetValue("Telegram:Port")),
                    SessionTag = "session", // by default
                    Properties = new ApplicationProperties
                    {
                        AppVersion = "1.0.0", // You can leave as in the example
                        DeviceModel = "PC", // You can leave as in the example
                        LangCode = "en", // You can leave as in the example
                        LangPack = "tdesktop", // You can leave as in the example
                        SystemLangCode = "en", // You can leave as in the example
                        SystemVersion = "Win 10 Pro" // You can leave as in the example
                    }
                };

                _clientApi = await ClientFactory.BuildClientAsync(settings).ConfigureAwait(false);
                _clientApi.KeepAliveConnection();
                if (!_clientApi.AuthService.CurrentUserId.HasValue)
                {
                    var hash = await _clientApi.AuthService.SendCodeAsync(_configuration.GetValue("Telegram:Phone")).ConfigureAwait(false);
                    var code = _userInteraction.InputPrompt("Please enter the Telegram code");
                    var user = await _clientApi.AuthService.SignInAsync(_configuration.GetValue("Telegram:Phone"), hash, code).ConfigureAwait(false);
                }
                var dialogs = await _clientApi.MessagesService.GetUserDialogsAsync(100).ConfigureAwait(false) as TDialogsSlice;
                foreach (OpenTl.Schema.IChat chat in dialogs.Chats)
                {
                    if (chat is TChannel)
                    {
                        //channelIds.Enqueue($"Channel/{(chat as TChannel).AccessHash}/{(chat as TChannel).Id}");
                    }
                    if (chat is TChat)
                    {
                        channelIds.Enqueue("Chat/" + (chat as TChat).Id.ToString());
                    }
                }
                finished = true;
            });
            task.Start();

            while (!finished)
            {
                string result;
                while (channelIds.TryDequeue(out result))
                {
                    yield return result;
                }
            }

        }

        public IEnumerable<DiscoveredFile> Discover(string path, HashSet<string> existingSources)
        {
            string[] pathParts = path.Split('/');


            ConcurrentQueue<DiscoveredFile> files = new ConcurrentQueue<DiscoveredFile>();

            bool finished = false;

            var task = new Task(async () =>
            {
                IInputPeer target = null;
                if (pathParts[0] == "Chat")
                {
                    int chatId = int.Parse(pathParts[1]);
                    target = new TInputPeerChat() { ChatId = chatId };
                }
                else if (pathParts[0] == "Channel")
                {
                    long accessHash = long.Parse(pathParts[1]);
                    int channelId = int.Parse(pathParts[2]);
                    target = new TInputPeerChannel() { AccessHash = accessHash, ChannelId = channelId };
                }

                int offset = 0;
                while (!finished)
                {
                    var chat = await _clientApi.MessagesService.GetHistoryAsync(target, offset, 100, 100).ConfigureAwait(false) as TMessagesSlice;

                    if (chat != null)
                    {
                        foreach (IMessage message in chat.Messages)
                        {
                            if (message is TMessage)
                            {
                                if ((message as TMessage).Media != null)
                                {
                                    string id = (message as TMessage).Id.ToString();
                                    files.Enqueue(new DiscoveredFile { Extension = "", Name = id, Path = id, Url = id });
                                }
                            }
                        }
                    }
                    else
                    {
                        finished = true;
                        return;
                    }
                    offset += 100;
                    finished = chat.Messages.Count == 0;
                }
            });
            task.Start();
            while (!finished)
            {
                DiscoveredFile result;
                while (files.TryDequeue(out result))
                {
                    yield return result;
                }
            }
        }

        public Task<string> GetMetadata(string path)
        {
            throw new NotImplementedException();
        }

        public Stream GetContents(DiscoveredFile file)
        {
            throw new NotImplementedException();
        }
    }
}
