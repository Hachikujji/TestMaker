using Newtonsoft.Json;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TestMaker.Database.Models;
using TestMaker.Stuff;
using TestMaker.UI.ViewModels;

namespace TestMaker.UI.Services
{
    public class TokenHandler : ITokenHandler
    {
        /// <summary>
        /// Is JWT token valid request async
        /// </summary>
        /// <returns>true if request is success,else returns false</returns>
        public async Task<bool> IsJwtTokenValidAsync()
        {
            HttpResponseMessage response = await StaticProperties.Client.GetAsync("user/validateToken");
            if (response.IsSuccessStatusCode)
                return true;
            else
                return false;
        }

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
                StaticProperties.Client.DefaultRequestHeaders.Remove("Authorization");
                StaticProperties.Client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", await response.Content.ReadAsStringAsync());
                return true;
            }
            else
                return false;
        }
    }
}