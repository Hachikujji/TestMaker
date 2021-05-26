﻿using Newtonsoft.Json;
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
        public DelegateCommand ReturnButtonEvent { get; }
        public DelegateCommand<object> StartTestButtonEvent { get; }

        private ObservableCollection<Test> _allowedTestList;

        public ObservableCollection<Test> AllowedTestList
        {
            get { return _allowedTestList; }
            set { SetProperty(ref _allowedTestList, value); }
        }

        private ITokenHandler _tokenHandler;

        public AllowedTestsWindowViewModel(IRegionManager regionManager, ITokenHandler tokenHandler) : base(regionManager)
        {
            ReturnButtonEvent = new DelegateCommand(ReturnButton);
            StartTestButtonEvent = new DelegateCommand<object>(StartTestButton);
            _tokenHandler = tokenHandler;
        }

        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (!(await TryGetAllowedTestList()))
            {
                if (await _tokenHandler.TryUpdateRefreshTokenAsync())
                    await TryGetAllowedTestList();
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
    }
}