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
    public class UserTestsWindowViewModel : ViewModelBase
    {
        #region Private Fields

        private ObservableCollection<Test> _testList;
        private ObservableCollection<Test> _testListFiltered;
        private ITokenHandler _tokenHandler;
        private Test _currentTest;
        private string _testFilter;
        private string _allowedUsersFilter;
        private string _allUsersFilter;

        #endregion Private Fields

        #region Public Constructors

        public UserTestsWindowViewModel(IRegionManager regionManager, ITokenHandler tokenHandler) : base(regionManager)
        {
            AllUsersFilter = "";
            TestFilter = "";
            AllowedUsersFilter = "";
            AllowedUsers = new ObservableCollection<string>();
            _tokenHandler = tokenHandler;
            DeleteTestButtonEvent = new DelegateCommand<object>(async (object test) => await DeleteTestButton(test));
            UpdateAllowedUsersButtonEvent = new DelegateCommand(async () => await UpdateAllowedUsersButton());
            AddAllowedUsersButtonEvent = new DelegateCommand<object>(async (object test) => await AddAllowedUsersButton(test));
            DeleteAllowedUsersButtonEvent = new DelegateCommand<object>(async (object test) => await DeleteAllowedUsersButton(test));
            EditTestButtonEvent = new DelegateCommand<object>(EditTestButton);
            ShowUsersResultsButtonEvent = new DelegateCommand<object>(ShowUsersResultsButton);
            ReturnButtonEvent = new DelegateCommand(ReturnButton);
        }

        #endregion Public Constructors

        #region Public Properties

        public ObservableCollection<Test> TestList
        {
            get { return _testList; }
            set { SetProperty(ref _testList, value); }
        }

        public ObservableCollection<Test> TestListFiltered
        {
            get { return _testListFiltered; }
            set { SetProperty(ref _testListFiltered, value); }
        }

        public ObservableCollection<string> AllowedUsers
        {
            get { return _allowedUsers; }
            set { SetProperty(ref _allowedUsers, value); }
        }

        public ObservableCollection<string> AllowedUsersFiltered
        {
            get { return _allowedUsersFiltered; }
            set { SetProperty(ref _allowedUsersFiltered, value); }
        }

        public ObservableCollection<string> AllUsers
        {
            get { return _allUsers; }
            set { SetProperty(ref _allUsers, value); }
        }

        public ObservableCollection<string> AllUsersFiltered
        {
            get { return _allUsersFiltered; }
            set { SetProperty(ref _allUsersFiltered, value); }
        }

        public Test SelectedTest
        {
            get { return _currentTest; }
            set { SetProperty(ref _currentTest, value); }
        }

        public string TestFilter
        {
            get { return _testFilter; }
            set { SetProperty(ref _testFilter, value); UpdateFilteredTestList(value); }
        }

        public string AllUsersFilter
        {
            get { return _allUsersFilter; }
            set { SetProperty(ref _allUsersFilter, value); UpdateFilteredAllUserList(value); }
        }

        public string AllowedUsersFilter
        {
            get { return _allowedUsersFilter; }
            set { SetProperty(ref _allowedUsersFilter, value); UpdateFilteredAllowedUserList(value); }
        }

        public DelegateCommand<object> DeleteTestButtonEvent { get; }
        public DelegateCommand UpdateAllowedUsersButtonEvent { get; }
        public DelegateCommand<object> AddAllowedUsersButtonEvent { get; }
        public DelegateCommand<object> DeleteAllowedUsersButtonEvent { get; }
        public DelegateCommand<object> EditTestButtonEvent { get; }
        public DelegateCommand<object> ShowUsersResultsButtonEvent { get; }
        public DelegateCommand ReturnButtonEvent { get; }

        private ObservableCollection<string> _allowedUsers;
        private ObservableCollection<string> _allowedUsersFiltered;
        private ObservableCollection<string> _allUsers;
        private ObservableCollection<string> _allUsersFiltered;

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// delete test button event
        /// </summary>
        /// <param name="testUI">test</param>
        /// <returns>task</returns>
        public async Task DeleteTestButton(object testUI)
        {
            if (testUI != null)
            {
                var test = (testUI as Test);
                if (!(await TryDeleteTest(test)))
                {
                    if (await _tokenHandler.TryRefreshTokenAsync())
                    {
                        await TryDeleteTest(test);
                    }
                    else
                    {
                        MessageBox.Show("Your token is expired.");
                    }
                }
            }
        }

        /// <summary>
        /// Update allowed users button event
        /// </summary>
        /// <returns>Task</returns>
        public async Task UpdateAllowedUsersButton()
        {
            if (!(await TryGetAllowedUsers()))
            {
                if (await _tokenHandler.TryRefreshTokenAsync())
                {
                    await TryGetAllowedUsers();
                }
                else
                {
                    MessageBox.Show("Your token is expired.");
                    RegionManager.RequestNavigate(StaticProperties.ContentRegion, "AuthorizationWindow");
                }
            }
        }

        /// <summary>
        /// Add allowed user button event
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>Task</returns>
        public async Task AddAllowedUsersButton(object username)
        {
            if (SelectedTest != null)
            {
                if (AllowedUsers.Contains(username as string))
                    return;
                if (!(await TryAddAllowedUser((username as string), SelectedTest)))
                {
                    if (await _tokenHandler.TryRefreshTokenAsync())
                    {
                        await TryAddAllowedUser((username as string), SelectedTest);
                    }
                    else
                    {
                        MessageBox.Show("Your token is expired.");
                        RegionManager.RequestNavigate(StaticProperties.ContentRegion, "AuthorizationWindow");
                    }
                }
            }
            else
                MessageBox.Show("Choose test");
        }

        /// <summary>
        /// Delete allowed user button event
        /// </summary>
        /// <param name="username">username</param>
        /// <returns>Task</returns>
        public async Task DeleteAllowedUsersButton(object username)
        {
            if (SelectedTest != null)
            {
                if (!(await TryDeleteAllowedUser((username as string), SelectedTest)))
                {
                    if (await _tokenHandler.TryRefreshTokenAsync())
                    {
                        await TryDeleteAllowedUser((username as string), SelectedTest);
                    }
                    else
                    {
                        MessageBox.Show("Your token is expired.");
                        RegionManager.RequestNavigate(StaticProperties.ContentRegion, "AuthorizationWindow");
                    }
                }
            }
            else
                MessageBox.Show("Choose test");
        }

        /// <summary>
        /// return button event
        /// </summary>
        public void ReturnButton()
        {
            RegionManager.RequestNavigate(StaticProperties.ContentRegion, "MenuHubWindow");
        }

        /// <summary>
        /// Edit test button event
        /// </summary>
        /// <param name="testUI">Test</param>
        public void EditTestButton(object testUI)
        {
            var test = (testUI as Test);
            NavigationParameters navigationParameters = null;
            if (test != null)
            {
                navigationParameters = new NavigationParameters();
                navigationParameters.Add("TestId", test.Id);
            }
            RegionManager.RequestNavigate(StaticProperties.ContentRegion, "EditTestWindow", navigationParameters);
        }

        /// <summary>
        /// Show users results button event
        /// </summary>
        /// <param name="testUI">Test</param>
        public void ShowUsersResultsButton(object testUI)
        {
            var test = (testUI as Test);
            NavigationParameters navigationParameters = null;
            if (test != null)
            {
                navigationParameters = new NavigationParameters();
                navigationParameters.Add("TestId", test.Id);
            }
            RegionManager.RequestNavigate(StaticProperties.ContentRegion, "TestResultsWindow", navigationParameters);
        }

        /// <summary>
        /// On navigated to UserControl event
        /// </summary>
        /// <param name="navigationContext">key=value args</param>
        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            AllUsersFilter = "";
            TestFilter = "";
            AllowedUsersFilter = "";
            AllowedUsersFiltered = null;
            if (!(await TryGetTestList()) || !(await TryGetUsernames()))
            {
                if (await _tokenHandler.TryRefreshTokenAsync())
                {
                    await TryGetTestList();
                    await TryGetUsernames();
                }
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
        /// try get test list
        /// </summary>
        /// <returns>true if request is success,else returns false</returns>
        private async Task<bool> TryGetTestList()
        {
            var response = await StaticProperties.Client.GetAsync("/test/getTestList/").ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                TestList = TestListFiltered = JsonConvert.DeserializeObject<ObservableCollection<Test>>(await response.Content.ReadAsStringAsync());
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// try add allowed user
        /// </summary>
        /// <returns>true if request is success,else returns false</returns>
        private async Task<bool> TryAddAllowedUser(string username, Test test)
        {
            var request = JsonConvert.SerializeObject(new AddTestAccessRequest(username, test));
            var response = await StaticProperties.Client.PostAsync("/test/addTaskAllowedUser/", new StringContent(request, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                AllowedUsers.Add(username);
                UpdateFilteredAllowedUserList(AllowedUsersFilter);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// try delete allowed user
        /// </summary>
        /// <returns>true if request is success,else returns false</returns>
        private async Task<bool> TryDeleteAllowedUser(string username, Test test)
        {
            var request = JsonConvert.SerializeObject(new AddTestAccessRequest(username, test));
            var response = await StaticProperties.Client.PostAsync("/test/deleteTaskAllowedUser/", new StringContent(request, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                UpdateFilteredAllowedUserList(AllowedUsersFilter);
                AllowedUsers.Remove(username);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// try get all usernames
        /// </summary>
        /// <returns>true if request is success,else returns false</returns>
        private async Task<bool> TryGetUsernames()
        {
            var response = await StaticProperties.Client.GetAsync("/user/getUsernames/").ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                AllUsersFiltered = AllUsers = JsonConvert.DeserializeObject<ObservableCollection<string>>(await response.Content.ReadAsStringAsync());
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// try get test allowed usernames
        /// </summary>
        /// <returns>true if request is success,else returns false</returns>
        private async Task<bool> TryGetAllowedUsers()
        {
            var request = JsonConvert.SerializeObject(SelectedTest);
            var response = await StaticProperties.Client.PostAsync($"/test/getTestAllowedUsers", new StringContent(request, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                AllowedUsers = JsonConvert.DeserializeObject<ObservableCollection<string>>(await response.Content.ReadAsStringAsync());
                UpdateFilteredAllowedUserList(AllowedUsersFilter);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// try delete test
        /// </summary>
        /// <returns>true if request is success,else returns false</returns>
        private async Task<bool> TryDeleteTest(Test test)
        {
            var request = JsonConvert.SerializeObject(test);
            var response = await StaticProperties.Client.PostAsync($"/test/deleteTest", new StringContent(request, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                TestList.Remove(test);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Update filters for test list
        /// </summary>
        private void UpdateFilteredTestList(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                TestListFiltered = TestList;
            else
                TestListFiltered = new ObservableCollection<Test>(TestList.Where(item => item.Name.Contains(filter)).ToList());
        }

        /// <summary>
        /// Update filters for all usernames list
        /// </summary>
        private void UpdateFilteredAllUserList(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                AllUsersFiltered = AllUsers;
            else
                AllUsersFiltered = new ObservableCollection<string>(AllUsers.Where(item => item.Contains(filter)).ToList());
        }

        /// <summary>
        /// Update filters for allowed user's usernames list
        /// </summary>
        private void UpdateFilteredAllowedUserList(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                AllowedUsersFiltered = AllowedUsers;
            else
                AllowedUsersFiltered = new ObservableCollection<string>(AllowedUsers.Where(item => item.Contains(filter)).ToList());
        }

        #endregion Private Methods
    }
}