using Newtonsoft.Json;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TestMaker.Database.Models;
using TestMaker.Stuff;
using TestMaker.UI.ViewModels;

namespace TestMaker.UI.Services
{
    public class TokenHandler : ITokenHandler
    {
        #region Public Methods

        /// <summary>
        /// try update JWT token async
        /// </summary>
        /// <returns>true if request is success,else returns false</returns>
        public async Task<bool> TryRefreshTokenAsync()
        {
            var json = JsonConvert.SerializeObject(StaticProperties.CurrentUserResponseHeader);
            HttpResponseMessage response = await StaticProperties.Client.PostAsync("user/refreshToken", new StringContent(json, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                var userResponce = JsonConvert.DeserializeObject<UserAuthenticationResponse>(await response.Content.ReadAsStringAsync());
                StaticProperties.CurrentUserResponseHeader = userResponce;
                StaticProperties.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", StaticProperties.CurrentUserResponseHeader.JwtToken);
                return true;
            }
            else
                return false;
        }

        #endregion Public Methods
    }
}