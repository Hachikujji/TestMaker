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

namespace TestMaker.Authorization.ViewModels
{
    public class AuthorizationWindowViewModel : BindableBase
    {
        private string _username;

        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        private string _password;

        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        public HttpClient Client { get; set; }

        public DelegateCommand LoginButtonEvent { get; }
        public DelegateCommand RegistrationButtonEvent { get; }

        public AuthorizationWindowViewModel()
        {
            //http client config
            Client = new HttpClient();
            Client.BaseAddress = new Uri("https://localhost:44396/");
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            LoginButtonEvent = new DelegateCommand(LoginButton);
            RegistrationButtonEvent = new DelegateCommand(RegistrationButton);
            Password = "pass";
        }

        public async void LoginButton()
        {
            var user = new User(Username, Password);
            HttpResponseMessage response = await Client.PostAsync("user/authorization", new StringContent(user.ToString()));
            if (response.IsSuccessStatusCode)
            {
                var list = await response.Content.ReadAsStringAsync();
                Password = list;
            }
            else
                Password = response.StatusCode.ToString();
        }

        public async void RegistrationButton()
        {
        }

        public async void GetUsersTestMethod()
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
    }
}