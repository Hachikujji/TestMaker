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
    public class UserTestResultsWindowViewModel : ViewModelBase
    {
        public DelegateCommand ReturnButtonEvent { get; }
        public DelegateCommand<object> ShowTestResultsButtonEvent { get; }

        private ObservableCollection<TestResult> _testResultList;

        public ObservableCollection<TestResult> TestResultList
        {
            get { return _testResultList; }
            set { SetProperty(ref _testResultList, value); }
        }

        private ITokenHandler _tokenHandler;

        public UserTestResultsWindowViewModel(IRegionManager regionManager, ITokenHandler tokenHandler) : base(regionManager)
        {
            ShowTestResultsButtonEvent = new DelegateCommand<object>(async (object testUI) => await ShowTestResultsButtonAsync(testUI));
            ReturnButtonEvent = new DelegateCommand(ReturnButton);
            _tokenHandler = tokenHandler;
        }

        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (!(await TryGetTestResultsList()))
            {
                if (await _tokenHandler.TryUpdateRefreshTokenAsync())
                    await TryGetTestResultsList();
                else
                {
                    MessageBox.Show("Your token is expired.");
                    RegionManager.RequestNavigate(StaticProperties.ContentRegion, "AuthorizationWindow");
                }
            }
        }

        public void ReturnButton()
        {
            RegionManager.RequestNavigate(StaticProperties.ContentRegion, "MenuHubWindow");
        }

        public async Task ShowTestResultsButtonAsync(object testResultUI)
        {
            var testResult = (testResultUI as TestResult);
            if (testResult != null && !(await TryGetIsUserCanCheckTestResult(testResult)))
            {
                if (await _tokenHandler.TryUpdateRefreshTokenAsync())
                    await TryGetIsUserCanCheckTestResult(testResult);
                else
                {
                    MessageBox.Show("Your token is expired.");
                    RegionManager.RequestNavigate(StaticProperties.ContentRegion, "AuthorizationWindow");
                }
            }
        }

        private async Task<bool> TryGetTestResultsList()
        {
            var response = await StaticProperties.Client.GetAsync("/test/getUserTestResultList").ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                TestResultList = JsonConvert.DeserializeObject<ObservableCollection<TestResult>>(await response.Content.ReadAsStringAsync());
                return true;
            }
            else
                return false;
        }

        private async Task<bool> TryGetIsUserCanCheckTestResult(TestResult testResult)
        {
            var request = JsonConvert.SerializeObject(new GetTestRequest(testResult.Test.Id));
            var response = await StaticProperties.Client.PostAsync("/test/isUserCanCheckTestResult", new StringContent(request, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                var boolJson = await response.Content.ReadAsStringAsync();
                var isCanCheck = JsonConvert.DeserializeObject<bool>(boolJson);
                if (isCanCheck)
                {
                    var navigationParameters = new NavigationParameters();
                    navigationParameters.Add("TestResultId", testResult.Id);
                    RegionManager.RequestNavigate(StaticProperties.ContentRegion, "PreviewRightTestAnswersWindow", navigationParameters);
                }
                else
                    MessageBox.Show("The correct answers to the test will be available after passing all attempts.");
                return true;
            }
            else
                return false;
        }
    }
}