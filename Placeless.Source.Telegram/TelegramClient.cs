using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenTl.ClientApi;
using OpenTl.ClientApi.MtProto.Exceptions;
using OpenTl.Schema;

namespace Placeless.Source.Telegram
{

    public class TelegramClient
    {
        private readonly IUserInteraction _userInteraction;
        private IClientApi _clientApi;
        private readonly IPlacelessconfig _configuration;

        public TelegramClient(IUserInteraction userInteraction, IPlacelessconfig configuration)
        {
            _configuration = configuration;
            _userInteraction = userInteraction;
        }



        public async Task InitAsync()
        {
            _clientApi.UpdatesService.AutoReceiveUpdates += update =>
            {
                Console.WriteLine($"updates: {JsonConvert.SerializeObject(update)}");
            };

            if (_clientApi.AuthService.CurrentUserId.HasValue)
            {
                _clientApi.UpdatesService.StartReceiveUpdates(TimeSpan.FromSeconds(1));
            }
        }

        public async Task LogOut()
        {
            await _clientApi.AuthService.LogoutAsync().ConfigureAwait(false);
        }

        public async Task SignIn(string phone)
        {
            var sentCode = await _clientApi.AuthService.SendCodeAsync(phone).ConfigureAwait(false);

            var code = _userInteraction.InputPrompt("Write a code:");
            TUser _user;

            try
            {
                _user = await _clientApi.AuthService.SignInAsync(phone, sentCode, code).ConfigureAwait(false);

                Console.WriteLine($"User login. Current user is {_user.FirstName} {_user.LastName}");
            }
            catch (CloudPasswordNeededException)
            {
                var passwordStr = _userInteraction.InputPrompt("Write a password:");
                throw new NotSupportedException();
                //                _user = await _clientApi.AuthService.CheckCloudPasswordAsync(passwordStr).ConfigureAwait(false);
            }
            catch (PhoneCodeInvalidException)
            {
            }

            _clientApi.UpdatesService.StartReceiveUpdates(TimeSpan.FromSeconds(1));
        }
    }
}
