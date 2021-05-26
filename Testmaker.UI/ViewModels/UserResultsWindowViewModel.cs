using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TestMaker.Database.Entities;
using TestMaker.Stuff;
using TestMaker.UI.Services;

namespace TestMaker.UI.ViewModels
{
    public class UserResultsWindowViewmodel : ViewModelBase
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

        public UserResultsWindowViewmodel(IRegionManager regionManager, ITokenHandler tokenHandler) : base(regionManager)
        {
            ReturnButtonEvent = new DelegateCommand(ReturnButton);
            ShowTestResultsButtonEvent = new DelegateCommand<object>(async (object testResult) => await ShowTestResultsButton(testResult));
            _tokenHandler = tokenHandler;
        }

        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (!((TestResultList = await TryGetTestResultsList()) == null))
            {
                if (await _tokenHandler.TryUpdateRefreshTokenAsync())
                    TestResultList = await TryGetTestResultsList();
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

        public async Task ShowTestResultsButton(object testResultUI)
        {
            var testResult = (testResultUI as TestResult);
            var navigationParameters = new NavigationParameters();
            navigationParameters.Add("TestResultId", testResult.Id);
            RegionManager.RequestNavigate(StaticProperties.ContentRegion, "CompletionTestWindow", navigationParameters);

        }

        private async Task<ObservableCollection<TestResult>> TryGetTestResultsList()
        {
            var response = await StaticProperties.Client.GetAsync("/test/getUserTestResultList").ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<ObservableCollection<TestResult>>(await response.Content.ReadAsStringAsync());
            else
                return null;
        }
    }
}