using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TestMaker.Database.Entities;
using TestMaker.Database.Models;
using Newtonsoft.Json;
using Prism.Regions;
using TestMaker.Common;
using System.Windows;
using System.Timers;
using System.Windows.Controls;

namespace TestMaker.UI.ViewModels
{
    public class AuthorizationWindowViewModel : ViewModelBase
    {
        #region Public Constructors

        public AuthorizationWindowViewModel(IRegionManager regionManager) : base(regionManager)
        {
            _errorTimer.AutoReset = false;
            _errorTimer.Interval = 2000;
            _errorTimer.Elapsed += ErrorTimerElapsedEvent;
            //http client config
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            Client.BaseAddress = new Uri("https://localhost:44336/");
            LoginButtonEvent = new DelegateCommand<object>(LoginButton);
            RegistrationButtonEvent = new DelegateCommand<object>(RegistrationButton);
            GetUsersEvent = new DelegateCommand(GetUsers);
            AuthorizationErrorLogVisibility = Visibility.Collapsed;
        }

        #endregion Public Constructors

        #region Public Properties

        public Visibility AuthorizationErrorLogVisibility
        {
            get => _authorizationErrorLogVisibility;
            set => SetProperty(ref _authorizationErrorLogVisibility, value);
        }

        public string AuthorizationError
        {
            get { return _authorizationError; }
            set { SetProperty(ref _authorizationError, value); }
        }

        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        public HttpClient Client { get; set; }

        public DelegateCommand<object> LoginButtonEvent { get; }

        public DelegateCommand<object> RegistrationButtonEvent { get; }

        public DelegateCommand GetUsersEvent { get; }

        #endregion Public Properties

        #region Public Methods

        public async void LoginButton(object PasswordBox)
        {
            string password = (PasswordBox as PasswordBox)?.Password;
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
                return;
            var userRequest = new UserAuthenticationRequest(Username, password);
            var json = JsonConvert.SerializeObject(userRequest);
            try
            {
                HttpResponseMessage response = await Client.PostAsync("user/authorization", new StringContent(json, Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    _currentUser = JsonConvert.DeserializeObject<UserAuthenticationResponse>(body);
                    _userAuthorizationRequest = new UserAuthorizationRequest(_currentUser);
                    Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_userAuthorizationRequest.ToString());
                    RegionManager.RequestNavigate(RegionNames.ContentRegion, "MenuHubWindow");
                }
                else
                {
                    AuthorizationError = "Wrong username or password";
                    _errorTimer.Start();
                }
            }
            catch (System.Net.Http.HttpRequestException e)
            {
                AuthorizationError = $"Server Not Responding: {e}";
                _errorTimer.Start();
            }
            (PasswordBox as PasswordBox).Password = string.Empty;
        }

        public async void RegistrationButton(object PasswordBox)
        {
            string password = (PasswordBox as PasswordBox)?.Password;
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
                return;
            var userRequest = new UserAuthenticationRequest(Username, password);
            var json = JsonConvert.SerializeObject(userRequest);
            try
            {
                HttpResponseMessage isUserExistsResponse = await Client.GetAsync($"/user/isUserExists/{Username}");
                if (!isUserExistsResponse.IsSuccessStatusCode)
                {
                    HttpResponseMessage addUserResponse = await Client.PostAsync("user/addUser", new StringContent(json, Encoding.UTF8, "application/json"));
                    LoginButton(PasswordBox);
                }
                else
                {
                    AuthorizationError = "User already exists";
                    _errorTimer.Start();
                }
            }
            catch (System.Net.Http.HttpRequestException e)
            {
                AuthorizationError = $"Server Not Responding: {e}";
                _errorTimer.Start();
            }
            (PasswordBox as PasswordBox).Password = string.Empty;
        }

        public void GetUsers()
        {
            //HttpResponseMessage response = await Client.GetAsync("getUsers");
            //if (response.IsSuccessStatusCode)
            //{
            //    var list = await response.Content.ReadAsStringAsync();
            //    Password = list;
            //}
            //else
            //    Password = response.StatusCode.ToString();
            RegionManager.RequestNavigate(RegionNames.ContentRegion, "MenuHubWindow");
        }

        private void ErrorTimerElapsedEvent(object sender, ElapsedEventArgs e)
        {
            AuthorizationError = String.Empty;
            _errorTimer.Stop();
        }

        #endregion Public Methods

        #region Private Fields

        private string _authorizationError;

        // Visibility of authorization error message
        private Visibility _authorizationErrorLogVisibility;

        private UserAuthenticationResponse _currentUser;
        private UserAuthorizationRequest _userAuthorizationRequest;

        private string _username;
        private Timer _errorTimer = new Timer();

        #endregion Private Fields
    }
}