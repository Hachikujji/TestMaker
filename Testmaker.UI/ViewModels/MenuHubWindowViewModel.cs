using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TestMaker.Database.Models;
using TestMaker.Stuff;

namespace TestMaker.UI.ViewModels
{
    public class MenuHubWindowViewModel : ViewModelBase
    {
        public DelegateCommand ReturnButtonEvent { get; }
        public DelegateCommand AddButtonEvent { get; }
        public DelegateCommand TestListButtonEvent { get; }
        public DelegateCommand AllowedTestListButtonEvent { get; }
        public DelegateCommand UserResultsButtonEvent { get; }

        public MenuHubWindowViewModel(IRegionManager regionManager) : base(regionManager)
        {
            ReturnButtonEvent = new DelegateCommand(ReturnButton);
            AddButtonEvent = new DelegateCommand(async () => await AddButton());
            TestListButtonEvent = new DelegateCommand(async () => await TestListButton());
            AllowedTestListButtonEvent = new DelegateCommand(async () => await AllowedTestListButton());
            UserResultsButtonEvent = new DelegateCommand(async () => await UserResultsButton());
        }

        public void ReturnButton()
        {
            RegionManager.RequestNavigate(StaticProperties.ContentRegion, "AuthorizationWindow");
        }

        public async Task AddButton()
        {
            await ValidateTokenAndNavigateTo("EditTestWindow");
        }

        public async Task TestListButton()
        {
            await ValidateTokenAndNavigateTo("UserTestsWindow");
        }

        public async Task AllowedTestListButton()
        {
            await ValidateTokenAndNavigateTo("AllowedTestsWindow");
        }

        public async Task UserResultsButton()
        {
            await ValidateTokenAndNavigateTo("UserTestResultsWindow");
        }

        private async Task ValidateTokenAndNavigateTo(string path)
        {
            HttpResponseMessage response = await StaticProperties.Client.GetAsync("user/validateToken");
            if (response.IsSuccessStatusCode)
                RegionManager.RequestNavigate(StaticProperties.ContentRegion, path);
            else
            {
                string json = null;
                try
                {
                    json = JsonConvert.SerializeObject(StaticProperties.CurrentUserResponseHeader);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    return;
                }

                response = await StaticProperties.Client.PostAsync("user/refreshToken", new StringContent(json, Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    var userResponce = JsonConvert.DeserializeObject<UserAuthenticationResponse>(await response.Content.ReadAsStringAsync());
                    StaticProperties.CurrentUserResponseHeader = userResponce;
                    StaticProperties.Client.DefaultRequestHeaders.Remove("Authorization");
                    StaticProperties.Client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", await response.Content.ReadAsStringAsync());
                    RegionManager.RequestNavigate(StaticProperties.ContentRegion, path);
                }
                else
                    RegionManager.RequestNavigate(StaticProperties.ContentRegion, "AuthorizationWindow");
            }
        }
    }
}