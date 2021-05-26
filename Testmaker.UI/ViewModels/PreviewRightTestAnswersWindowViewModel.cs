using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
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
    public class PreviewRightTestAnswersWindowViewModel : ViewModelBase
    {
        private ITokenHandler _tokenHandler;

        private TestResult _test;

        public DelegateCommand ReturnButtonEvent { get; }

        public TestResult TestResult
        {
            get { return _test; }
            set { SetProperty(ref _test, value); }
        }

        public PreviewRightTestAnswersWindowViewModel(IRegionManager regionManager, ITokenHandler tokenHandler) : base(regionManager)
        {
            ReturnButtonEvent = new DelegateCommand(ReturnButton);
            _tokenHandler = tokenHandler;
        }

        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            TestResult = null;
            int testId = 0;
            if (navigationContext.Parameters.Count != 0)
            {
                string TestResultIdString = navigationContext.Parameters["TestResultId"].ToString();
                if (!string.IsNullOrWhiteSpace(TestResultIdString) && int.TryParse(TestResultIdString, out testId))
                    if (!await TryGetTestResult(testId))
                    {
                        if (await _tokenHandler.TryUpdateRefreshTokenAsync())
                        {
                            await TryGetTestResult(testId);
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
                MessageBox.Show($"Test result id not provided.");
                RegionManager.RequestNavigate(StaticProperties.ContentRegion, "MenuHubWindow");
            }
        }

        private async Task<bool> TryGetTestResult(int testResultId)
        {
            var request = JsonConvert.SerializeObject(new GetTestRequest(testResultId));
            var response = await StaticProperties.Client.PostAsync("/test/getTestResult", new StringContent(request, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var testJson = await response.Content.ReadAsStringAsync();
                    var test = JsonConvert.DeserializeObject<TestResult>(testJson);
                    if (test != null)
                        TestResult = test;
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

        public void ReturnButton()
        {
            RegionManager.RequestNavigate(StaticProperties.ContentRegion, "MenuHubWindow");
        }
    }
}