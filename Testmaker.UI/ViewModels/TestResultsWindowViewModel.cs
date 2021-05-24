using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class TestResultsWindowViewModel : ViewModelBase
    {
        public DelegateCommand ReturnButtonEvent { get; }
        public DelegateCommand<object> StartTestButtonEvent { get; }

        private ObservableCollection<TestResult> _testResultList;

        public ObservableCollection<TestResult> TestResultList
        {
            get { return _testResultList; }
            set { SetProperty(ref _testResultList, value); }
        }

        private ITokenHandler _tokenHandler;

        public TestResultsWindowViewModel(IRegionManager regionManager, ITokenHandler tokenHandler) : base(regionManager)
        {
            ReturnButtonEvent = new DelegateCommand(ReturnButton);
            StartTestButtonEvent = new DelegateCommand<object>(StartTestButton);
            _tokenHandler = tokenHandler;
        }

        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            int editTestId = 0;
            string editTestIdString;
            if (navigationContext.Parameters.Count != 0)
            {
                editTestIdString = navigationContext.Parameters["TestId"].ToString();
                if (!string.IsNullOrWhiteSpace(editTestIdString))
                    int.TryParse(editTestIdString, out editTestId);
                if (editTestId > 0)
                    if (!await TryGetTestResultsList(editTestId))
                    {
                        if (await _tokenHandler.TryUpdateRefreshTokenAsync())
                        {
                            await TryGetTestResultsList(editTestId);
                        }
                        else
                        {
                            MessageBox.Show("Your token is expired.");
                            RegionManager.RequestNavigate(StaticProperties.ContentRegion, "AuthorizationWindow");
                        }
                    }
            }
        }

        public void ReturnButton()
        {
            RegionManager.RequestNavigate(StaticProperties.ContentRegion, "MenuHubWindow");
        }

        public void StartTestButton(object testUI)
        {
            var test = (testUI as Test);
            NavigationParameters navigationParameters = null;
            if (test != null)
            {
                MessageBoxResult dialogResult = MessageBox.Show($"Do you want to start test: {test.Name}", "Confirm action", MessageBoxButton.YesNo);
                if (dialogResult == MessageBoxResult.Yes)
                {
                    navigationParameters = new NavigationParameters();
                    navigationParameters.Add("TestId", test.Id);
                    RegionManager.RequestNavigate(StaticProperties.ContentRegion, "CompletionTestWindow", navigationParameters);
                }
            }
        }

        private async Task<bool> TryGetTestResultsList(int testId)
        {
            var json = JsonConvert.SerializeObject(new GetTestRequest(testId));
            var response = await StaticProperties.Client.PostAsync("/test/getTestResultList", new StringContent(json, Encoding.UTF8, "application/json")).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                TestResultList = JsonConvert.DeserializeObject<ObservableCollection<TestResult>>(await response.Content.ReadAsStringAsync());
                return true;
            }
            else
                return false;
        }
    }
}