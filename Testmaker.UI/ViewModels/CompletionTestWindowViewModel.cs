using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TestMaker.Database.Entities;
using TestMaker.Database.Models;
using TestMaker.Stuff;
using TestMaker.UI.Services;

namespace TestMaker.UI.ViewModels
{
    public class CompletionTestWindowViewModel : ViewModelBase
    {
        #region Private Fields

        private readonly ITokenHandler _tokenHandler;

        private TestResult _test;

        #endregion Private Fields

        #region Public Constructors

        public CompletionTestWindowViewModel(IRegionManager regionManager, ITokenHandler tokenHandler) : base(regionManager)
        {
            SendTestResultButtonCommand = new DelegateCommand(async () => await SendTestResultButton());
            _tokenHandler = tokenHandler;
        }

        #endregion Public Constructors

        #region Public Properties

        public TestResult Test
        {
            get { return _test; }
            set { SetProperty(ref _test, value); }
        }

        public DelegateCommand SendTestResultButtonCommand { get; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Navigated to UserControl event
        /// </summary>
        /// <param name="navigationContext">key=value vars</param>
        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            int testId = 0;
            string TestIdString = navigationContext.Parameters["TestId"].ToString();
            if (!string.IsNullOrWhiteSpace(TestIdString))
                if (int.TryParse(TestIdString, out testId))
                    if (!await TryGetTest(testId))
                    {
                        if (await _tokenHandler.TryRefreshTokenAsync())
                        {
                            await TryGetTest(testId);
                        }
                        else
                        {
                            MessageBox.Show(LocalizationService.GetLocalizedValue<string>("TokenExpired"));
                            RegionManager.RequestNavigate(StaticProperties.ContentRegion, "AuthorizationWindow");
                        }
                    }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// try get test by id
        /// </summary>
        /// <returns>true if request is success,else returns false</returns>
        private async Task<bool> TryGetTest(int testId)
        {
            var request = JsonConvert.SerializeObject(testId);
            var response = await StaticProperties.Client.PostAsync("/test/getTest", new StringContent(request, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                var testJson = await response.Content.ReadAsStringAsync();
                var test = JsonConvert.DeserializeObject<Test>(testJson);
                if (test != null)
                    Test = new TestResult(test);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// try send test
        /// </summary>
        /// <returns>true if request is success,else returns false</returns>
        private async Task<bool> TrySendTest()
        {
            var request = JsonConvert.SerializeObject(Test);
            var response = await StaticProperties.Client.PostAsync("/test/addTestResult", new StringContent(request, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                RegionManager.RequestNavigate(StaticProperties.ContentRegion, "MenuHubWindow");
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// send test button event
        /// </summary>
        /// <returns></returns>
        private async Task SendTestResultButton()
        {
            Test.Date = DateTime.Now;
            if (!await TrySendTest())
            {
                if (await _tokenHandler.TryRefreshTokenAsync())
                {
                    await TrySendTest();
                }
                else
                {
                    MessageBox.Show(LocalizationService.GetLocalizedValue<string>("TokenExpired"));
                    RegionManager.RequestNavigate(StaticProperties.ContentRegion, "AuthorizationWindow");
                }
            }
        }

        #endregion Private Methods
    }
}