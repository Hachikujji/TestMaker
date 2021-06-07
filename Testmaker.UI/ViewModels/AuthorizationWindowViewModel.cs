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
using TestMaker.Stuff;
using System.Windows;
using System.Timers;
using System.Windows.Controls;
using System.Security;
using WPFLocalizeExtension.Engine;
using TestMaker.UI.Services;

namespace TestMaker.UI.ViewModels
{
    public class AuthorizationWindowViewModel : ViewModelBase
    {
        #region Private Fields

        private const int _timerInterval = 2000;
        private string _authorizationError;

        // Visibility of authorization error message
        private Visibility _authorizationErrorLogVisibility;

        // Selected language
        private KeyValuePair<string, string> _selectedLanguage;

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
            _errorTimer.Interval = _timerInterval;
            _errorTimer.Elapsed += ErrorTimerElapsedEvent;

            LoginButtonCommand = new DelegateCommand(LoginButton);
            RegistrationButtonCommand = new DelegateCommand(RegistrationButton);
            LanguageChangedCommand = new DelegateCommand(LanguageChanged);
            AuthorizationErrorLogVisibility = Visibility.Collapsed;

            // HttpClient setup
            StaticProperties.Client = new HttpClient();
            StaticProperties.Client.DefaultRequestHeaders.Accept.Clear();
            StaticProperties.Client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            StaticProperties.Client.BaseAddress = new Uri("https://localhost:44336/");

            // language setup
            LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
            LocalizeDictionary.Instance.SetCultureCommand.Execute("en");
            Languages = new Dictionary<string, string>
            {
                {
                    "Русский", "ru"
                },
                {
                    "English", "en"
                }
            };
        }

        #endregion Public Constructors

        #region Public Properties

        public Visibility AuthorizationErrorLogVisibility
        {
            get => _authorizationErrorLogVisibility;
            set => SetProperty(ref _authorizationErrorLogVisibility, value);
        }

        public Dictionary<string, string> Languages { get; set; }

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

        public KeyValuePair<string, string> SelectedLanguage
        {
            get => _selectedLanguage;
            set => SetProperty(ref _selectedLanguage, value);
        }

        public DelegateCommand LoginButtonCommand { get; }
        public DelegateCommand RegistrationButtonCommand { get; }
        public DelegateCommand LanguageChangedCommand { get; }

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
                    StaticProperties.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", StaticProperties.CurrentUserResponseHeader.JwtToken);
                    RegionManager.RequestNavigate(StaticProperties.ContentRegion, "MenuHubWindow");
                }
                else
                {
                    AuthorizationError = LocalizationService.GetLocalizedValue<string>("AuthorizationError");
                    _errorTimer.Start();
                }
            }
            catch (Exception)
            {
                AuthorizationError = LocalizationService.GetLocalizedValue<string>("ServerError");
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
                AuthorizationError = LocalizationService.GetLocalizedValue<string>("AuthorizationError");
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
                        AuthorizationError = LocalizationService.GetLocalizedValue<string>("UserAlreadyExists");
                        _errorTimer.Start();
                    }
                }
            }
            catch (Exception)
            {
                AuthorizationError = LocalizationService.GetLocalizedValue<string>("ServerError");
                _errorTimer.Start();
            }
            Password = string.Empty;
        }

        /// <summary>
        /// Language change button
        /// </summary>
        public void LanguageChanged()
        {
            LocalizeDictionary.Instance.SetCultureCommand.Execute(SelectedLanguage.Value);
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