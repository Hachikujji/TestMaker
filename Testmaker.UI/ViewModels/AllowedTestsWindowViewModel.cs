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
    public class AllowedTestsWindowViewModel : ViewModelBase
    {
        #region Private Fields

        private readonly ITokenHandler _tokenHandler;
        private ObservableCollection<Test> _allowedTestList;

        #endregion Private Fields

        #region Public Constructors

        public AllowedTestsWindowViewModel(IRegionManager regionManager, ITokenHandler tokenHandler) : base(regionManager)
        {
            ReturnButtonCommand = new DelegateCommand(ReturnButton);
            StartTestButtonCommand = new DelegateCommand<object>(StartTestButton);
            _tokenHandler = tokenHandler;
        }

        #endregion Public Constructors

        #region Public Properties

        public DelegateCommand ReturnButtonCommand { get; }
        public DelegateCommand<object> StartTestButtonCommand { get; }

        public ObservableCollection<Test> AllowedTestList
        {
            get { return _allowedTestList; }
            set { SetProperty(ref _allowedTestList, value); }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Navigated to UserControl event
        /// </summary>
        /// <param name="navigationContext">key=value vars</param>
        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (!(await TryGetAllowedTestList()))
            {
                if (await _tokenHandler.TryRefreshTokenAsync())
                    await TryGetAllowedTestList();
                else
                {
                    MessageBox.Show(LocalizationService.GetLocalizedValue<string>("TokenExpired"));
                    RegionManager.RequestNavigate(StaticProperties.ContentRegion, "AuthorizationWindow");
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

        /// <summary>
        /// start test button event
        /// </summary>
        /// <param name="testUI">Test</param>
        public void StartTestButton(object testUI)
        {
            var test = (testUI as Test);
            if (test != null)
            {
                MessageBoxResult dialogResult = MessageBox.Show(string.Format(LocalizationService.GetLocalizedValue<string>("TestStartConfirm"), test.Name), LocalizationService.GetLocalizedValue<string>("ConfirmAction"), MessageBoxButton.YesNo);
                if (dialogResult == MessageBoxResult.Yes)
                {
                    NavigationParameters navigationParameters = new NavigationParameters();
                    navigationParameters.Add("TestId", test.Id);
                    RegionManager.RequestNavigate(StaticProperties.ContentRegion, "CompletionTestWindow", navigationParameters);
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// try get allowed test list
        /// </summary>
        /// <returns>true if request is success,else returns false</returns>
        private async Task<bool> TryGetAllowedTestList()
        {
            var response = await StaticProperties.Client.GetAsync("/test/getAllowedTestList/").ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                AllowedTestList = JsonConvert.DeserializeObject<ObservableCollection<Test>>(await response.Content.ReadAsStringAsync());
                return true;
            }
            else
                return false;
        }

        #endregion Private Methods
    }
}