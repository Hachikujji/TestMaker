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
        #region Private Fields

        private ObservableCollection<TestResult> _testResultList;
        private readonly ITokenHandler _tokenHandler;

        #endregion Private Fields

        #region Public Constructors

        public TestResultsWindowViewModel(IRegionManager regionManager, ITokenHandler tokenHandler) : base(regionManager)
        {
            ReturnButtonCommand = new DelegateCommand(ReturnButton);
            _tokenHandler = tokenHandler;
        }

        #endregion Public Constructors

        #region Public Properties

        public DelegateCommand ReturnButtonCommand { get; }

        public ObservableCollection<TestResult> TestResultList
        {
            get { return _testResultList; }
            set { SetProperty(ref _testResultList, value); }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// navigated to UserControl event
        /// </summary>
        /// <param name="navigationContext">key=value vars</param>
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
                        if (await _tokenHandler.TryRefreshTokenAsync())
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

        /// <summary>
        /// return button event
        /// </summary>
        public void ReturnButton()
        {
            RegionManager.RequestNavigate(StaticProperties.ContentRegion, "MenuHubWindow");
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// try get test results list
        /// </summary>
        /// <returns>true if request is success,else returns false</returns>
        private async Task<bool> TryGetTestResultsList(int testId)
        {
            var json = JsonConvert.SerializeObject(testId);
            var response = await StaticProperties.Client.PostAsync("/test/getTestResultList", new StringContent(json, Encoding.UTF8, "application/json")).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                TestResultList = JsonConvert.DeserializeObject<ObservableCollection<TestResult>>(await response.Content.ReadAsStringAsync());
                return true;
            }
            else
                return false;
        }

        #endregion Private Methods
    }
}