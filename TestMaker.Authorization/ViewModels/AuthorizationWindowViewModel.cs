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

namespace TestMaker.Authorization.ViewModels
{
    public class AuthorizationWindowViewModel : BindableBase
    {
        #region Public Constructors

        public AuthorizationWindowViewModel()
        {
            //http client config
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            Client.BaseAddress = new Uri("https://localhost:44396/");
            LoginButtonEvent = new DelegateCommand(LoginButton);
            RegistrationButtonEvent = new DelegateCommand(RegistrationButton);
            GetUsersEvent = new DelegateCommand(GetUsers);
        }

        #endregion Public Constructors

        #region Public Properties

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

        public HttpClient Client { get; set; }
        public DelegateCommand LoginButtonEvent { get; }
        public DelegateCommand RegistrationButtonEvent { get; }
        public DelegateCommand GetUsersEvent { get; }

        #endregion Public Properties

        #region Public Methods

        public async void LoginButton()
        {
            var userRequest = new UserAuthenticationRequest(Username, Password);
            var json = JsonConvert.SerializeObject(userRequest);
            HttpResponseMessage response = await Client.PostAsync("user/authorization", new StringContent(json, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _currentUser = JsonConvert.DeserializeObject<UserAuthenticationResponse>(body);
                _userAuthorizationRequest = new UserAuthorizationRequest(_currentUser);
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_userAuthorizationRequest.ToString());
            }
            else
                Console.WriteLine(response.StatusCode.ToString());

            RegionManager.RequestNavigate(RegionNames.ContentRegion, "Authorization");
        }

        public async void RegistrationButton()
        {
        }

        public async void GetUsers()
        {
            HttpResponseMessage response = await Client.GetAsync("getUsers");
            if (response.IsSuccessStatusCode)
            {
                var list = await response.Content.ReadAsStringAsync();
                Password = list;
            }
            else
                Password = response.StatusCode.ToString();
        }

        #endregion Public Methods

        #region Private Fields

        private UserAuthenticationResponse _currentUser;
        private UserAuthorizationRequest _userAuthorizationRequest;

        private string _username;
        private string _password;

        #endregion Private Fields
    }
}