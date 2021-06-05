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
using System.Windows;
using TestMaker.Database.Models;
using TestMaker.Stuff;

namespace TestMaker.UI.ViewModels
{
    public class MenuHubWindowViewModel : ViewModelBase
    {
        #region Public Constructors

        public MenuHubWindowViewModel(IRegionManager regionManager) : base(regionManager)
        {
            ReturnButtonCommand = new DelegateCommand(ReturnButton);
            AddButtonCommand = new DelegateCommand(async () => await AddButton());
            TestListButtonCommand = new DelegateCommand(async () => await TestListButton());
            AllowedTestListButtonCommand = new DelegateCommand(async () => await AllowedTestListButton());
            UserResultsButtonCommand = new DelegateCommand(async () => await UserResultsButton());
        }

        #endregion Public Constructors

        #region Public Properties

        public DelegateCommand ReturnButtonCommand { get; }
        public DelegateCommand AddButtonCommand { get; }
        public DelegateCommand TestListButtonCommand { get; }
        public DelegateCommand AllowedTestListButtonCommand { get; }
        public DelegateCommand UserResultsButtonCommand { get; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// return button event
        /// </summary>
        public void ReturnButton()
        {
            RegionManager.RequestNavigate(StaticProperties.ContentRegion, "AuthorizationWindow");
        }

        /// <summary>
        /// new test button event
        /// </summary>
        /// <returns>Task</returns>
        public async Task AddButton()
        {
            await ValidateTokenAndNavigateTo("EditTestWindow");
        }

        /// <summary>
        /// test list button event
        /// </summary>
        /// <returns>Task</returns>
        public async Task TestListButton()
        {
            await ValidateTokenAndNavigateTo("UserTestsWindow");
        }

        /// <summary>
        /// Allowed test list button event
        /// </summary>
        /// <returns>Task</returns>
        public async Task AllowedTestListButton()
        {
            await ValidateTokenAndNavigateTo("AllowedTestsWindow");
        }

        /// <summary>
        /// User results button event
        /// </summary>
        /// <returns>Task</returns>
        public async Task UserResultsButton()
        {
            await ValidateTokenAndNavigateTo("UserTestResultsWindow");
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// validate token, if token is valid - navigate to UserControl, else try to refresh token, if refresh token is expired return to authorization window.
        /// </summary>
        /// <returns>Task</returns>
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
                catch (Newtonsoft.Json.JsonReaderException e)
                {
                    MessageBox.Show($"Error when tried to convert user responce header: {e}");
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

        #endregion Private Methods
    }
}