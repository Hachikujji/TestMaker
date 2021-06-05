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
        #region Private Fields

        private ObservableCollection<TestResult> _testResultList;
        private readonly ITokenHandler _tokenHandler;

        #endregion Private Fields

        #region Public Constructors

        public UserTestResultsWindowViewModel(IRegionManager regionManager, ITokenHandler tokenHandler) : base(regionManager)
        {
            ShowTestResultsButtonCommand = new DelegateCommand<object>(async (object testUI) => await ShowTestResultsButtonAsync(testUI));
            ReturnButtonCommand = new DelegateCommand(ReturnButton);
            _tokenHandler = tokenHandler;
        }

        #endregion Public Constructors

        #region Public Properties

        public DelegateCommand ReturnButtonCommand { get; }
        public DelegateCommand<object> ShowTestResultsButtonCommand { get; }

        public ObservableCollection<TestResult> TestResultList
        {
            get { return _testResultList; }
            set { SetProperty(ref _testResultList, value); }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Navigated to UserControl event
        /// </summary>
        /// <param name="navigationContext">key=value vars</param>
        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (!(await TryGetTestResultsList()))
            {
                if (await _tokenHandler.TryRefreshTokenAsync())
                    await TryGetTestResultsList();
                else
                {
                    MessageBox.Show("Your token is expired.");
                    RegionManager.RequestNavigate(StaticProperties.ContentRegion, "AuthorizationWindow");
                }
            }
        }

        /// <summary>
        /// Return button event
        /// </summary>
        public void ReturnButton()
        {
            RegionManager.RequestNavigate(StaticProperties.ContentRegion, "MenuHubWindow");
        }

        /// <summary>
        /// <para>Show test results button event</para>
        /// if user has spelt all attemps he can see correct answers on test
        /// </summary>
        /// <param name="testResultUI"></param>
        /// <returns></returns>
        public async Task ShowTestResultsButtonAsync(object testResultUI)
        {
            var testResult = (testResultUI as TestResult);
            if (testResult != null && !(await TryGetIsUserCanCheckTestResult(testResult)))
            {
                if (await _tokenHandler.TryRefreshTokenAsync())
                    await TryGetIsUserCanCheckTestResult(testResult);
                else
                {
                    MessageBox.Show("Your token is expired.");
                    RegionManager.RequestNavigate(StaticProperties.ContentRegion, "AuthorizationWindow");
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// try get test results list
        /// </summary>
        /// <returns>true if request is success,else returns false</returns>
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

        /// <summary>
        /// try get is user can check test answers
        /// </summary>
        /// <returns>true if request is success,else returns false</returns>
        private async Task<bool> TryGetIsUserCanCheckTestResult(TestResult testResult)
        {
            var request = JsonConvert.SerializeObject(testResult.Test.Id);
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

        #endregion Private Methods
    }
}