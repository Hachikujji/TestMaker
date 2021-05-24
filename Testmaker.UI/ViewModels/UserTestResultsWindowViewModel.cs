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
    public class UserTestResultsWindowViewModel : ViewModelBase
    {
        public DelegateCommand ReturnButtonEvent { get; }

        private ObservableCollection<TestResult> _testResultList;

        public ObservableCollection<TestResult> TestResultList
        {
            get { return _testResultList; }
            set { SetProperty(ref _testResultList, value); }
        }

        private ITokenHandler _tokenHandler;

        public UserTestResultsWindowViewModel(IRegionManager regionManager, ITokenHandler tokenHandler) : base(regionManager)
        {
            ReturnButtonEvent = new DelegateCommand(ReturnButton);
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