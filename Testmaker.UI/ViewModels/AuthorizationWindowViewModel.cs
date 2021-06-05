﻿using Prism.Commands;
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
using TestMaker.Stuff;
using System.Windows;
using System.Timers;
using System.Windows.Controls;
using System.Security;

namespace TestMaker.UI.ViewModels
{
    public class AuthorizationWindowViewModel : ViewModelBase
    {
        #region Private Fields

        private string _authorizationError;

        // Visibility of authorization error message
        private Visibility _authorizationErrorLogVisibility;

        private UserAuthorizationRequest _userAuthorizationRequest;

        private string _username;
        private string _password;
        private Timer _errorTimer = new Timer();

        #endregion Private Fields

        #region Public Constructors

        public AuthorizationWindowViewModel(IRegionManager regionManager) : base(regionManager)
        {
            // timer setup
            _errorTimer.AutoReset = false;
            _errorTimer.Interval = 2000;
            _errorTimer.Elapsed += ErrorTimerElapsedEvent;

            LoginButtonCommand = new DelegateCommand(LoginButton);
            RegistrationButtonCommand = new DelegateCommand(RegistrationButton);
            AuthorizationErrorLogVisibility = Visibility.Collapsed;

            // HttpClient setup
            StaticProperties.Client = new HttpClient();
            StaticProperties.Client.DefaultRequestHeaders.Accept.Clear();
            StaticProperties.Client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            StaticProperties.Client.BaseAddress = new Uri("https://localhost:44336/");
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

        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        public DelegateCommand LoginButtonCommand { get; }

        public DelegateCommand RegistrationButtonCommand { get; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Login button event
        /// </summary>
        /// <param name="PasswordUI">PasswordBox.Password string</param>
        public async void LoginButton()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                return;
            var userRequest = new UserAuthenticationRequest(Username, Password);
            var json = JsonConvert.SerializeObject(userRequest);
            try
            {
                HttpResponseMessage response = await StaticProperties.Client.PostAsync("user/authorization", new StringContent(json, Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var _currentUser = JsonConvert.DeserializeObject<UserAuthenticationResponse>(body);
                    StaticProperties.CurrentUserResponseHeader = _currentUser;
                    _userAuthorizationRequest = new UserAuthorizationRequest(_currentUser);
                    var authString = JsonConvert.SerializeObject(_userAuthorizationRequest);
                    StaticProperties.Client.DefaultRequestHeaders.Remove("Authorization");
                    StaticProperties.Client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authString);
                    RegionManager.RequestNavigate(StaticProperties.ContentRegion, "MenuHubWindow");
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
            Password = string.Empty;
        }

        /// <summary>
        /// Registration button event
        /// </summary>
        /// <param name="PasswordUI">PasswordBox.Password string</param>
        public async void RegistrationButton()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                AuthorizationError = "Username or password is empty";
                _errorTimer.Start();
                return;
            }
            var userRequest = new UserAuthenticationRequest(Username, Password);
            var json = JsonConvert.SerializeObject(userRequest);
            try
            {
                HttpResponseMessage isUserExistsResponse = await StaticProperties.Client.GetAsync($"/user/isUserExists/{Username}");
                if (isUserExistsResponse.IsSuccessStatusCode)
                {
                    var isUserExistsString = await isUserExistsResponse.Content.ReadAsStringAsync();
                    var isUserExists = JsonConvert.DeserializeObject<bool>(isUserExistsString);
                    if (!isUserExists)
                    {
                        HttpResponseMessage addUserResponse = await StaticProperties.Client.PostAsync("user/addUser", new StringContent(json, Encoding.UTF8, "application/json"));
                        LoginButton();
                    }
                    else
                    {
                        AuthorizationError = "User already exists";
                        _errorTimer.Start();
                    }
                }
            }
            catch (System.Net.Http.HttpRequestException e)
            {
                AuthorizationError = $"Server Not Responding: {e}";
                _errorTimer.Start();
            }
            Password = string.Empty;
        }

        /// <summary>
        /// When error message should hide | timer elapsed event
        /// </summary>
        private void ErrorTimerElapsedEvent(object sender, ElapsedEventArgs e)
        {
            AuthorizationError = string.Empty;
            _errorTimer.Stop();
        }

        #endregion Public Methods
    }
}