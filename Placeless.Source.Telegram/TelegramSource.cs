using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenTl.ClientApi;
using OpenTl.Schema;
using OpenTl.Schema.Messages;
using static TdLib.TdApi;
using TdLib;
using Newtonsoft.Json;

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

        public async Task Discover()
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

            _clientApi = await ClientFactory.BuildClientAsync(settings);
            _clientApi.KeepAliveConnection();
            if (!_clientApi.AuthService.CurrentUserId.HasValue)
            {
                var hash = await _clientApi.AuthService.SendCodeAsync(_configuration.GetValue("Telegram:Phone"));
                var code = _userInteraction.InputPrompt("Please enter the Telegram code");
                var user = await _clientApi.AuthService.SignInAsync(_configuration.GetValue("Telegram:Phone"), hash, code);
            }

            var dialogs = await _clientApi.MessagesService.GetUserDialogsAsync(100) as TDialogsSlice;

            foreach(TChat chat in dialogs.Chats)
            {
                await Discover(chat.Id);
            }
            return;
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
    }
}
