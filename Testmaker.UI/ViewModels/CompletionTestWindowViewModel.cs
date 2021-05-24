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
        private ITokenHandler _tokenHandler;

        private TestResult _test;

        public TestResult Test
        {
            get { return _test; }
            set { SetProperty(ref _test, value); }
        }

        public DelegateCommand SendTestResultButtonEvent { get; }

        public CompletionTestWindowViewModel(IRegionManager regionManager, ITokenHandler tokenHandler) : base(regionManager)
        {
            SendTestResultButtonEvent = new DelegateCommand(async () => await SendTestButton());
            _tokenHandler = tokenHandler;
        }

        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            int testId = 0;
            if (navigationContext.Parameters.Count != 0)
            {
                string TestIdString = navigationContext.Parameters["TestId"].ToString();
                if (!string.IsNullOrWhiteSpace(TestIdString))
                    int.TryParse(TestIdString, out testId);

                if (!await TryGetTest(testId))
                {
                    if (await _tokenHandler.TryUpdateRefreshTokenAsync())
                    {
                        await TryGetTest(testId);
                    }
                    else
                    {
                        MessageBox.Show("Your token is expired.");
                        RegionManager.RequestNavigate(StaticProperties.ContentRegion, "AuthorizationWindow");
                    }
                }
            }
            else
            {
                MessageBox.Show($"Error when tried to get test.");
                RegionManager.RequestNavigate(StaticProperties.ContentRegion, "MenuHubWindow");
            }
        }

        private async Task<bool> TryGetTest(int testId)
        {
            var request = JsonConvert.SerializeObject(new GetTestRequest(testId));
            var response = await StaticProperties.Client.PostAsync("/test/getTest", new StringContent(request, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var testJson = await response.Content.ReadAsStringAsync();
                    var test = JsonConvert.DeserializeObject<Test>(testJson);
                    if (test != null)
                        Test = new TestResult(test);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error when tried to get test: {e}");
                }
                return true;
            }
            else
                return false;
        }

        private async Task<bool> TrySendTest()
        {
            var request = JsonConvert.SerializeObject(Test);
            var response = await StaticProperties.Client.PostAsync("/test/sendTestResult", new StringContent(request, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                RegionManager.RequestNavigate(StaticProperties.ContentRegion, "MenuHubWindow");
                return true;
            }
            else
                return false;
        }

        private async Task SendTestButton()
        {
            Test.Date = DateTime.Now;
            if (!await TrySendTest())
            {
                if (await _tokenHandler.TryUpdateRefreshTokenAsync())
                {
                    await TrySendTest();
                }
                else
                {
                    MessageBox.Show("Your token is expired.");
                    RegionManager.RequestNavigate(StaticProperties.ContentRegion, "AuthorizationWindow");
                }
            }
        }
    }
}