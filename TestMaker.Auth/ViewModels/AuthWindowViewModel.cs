using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace TestMaker.Auth.ViewModels
{
    public class AuthWindowViewModel : BindableBase
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

        public DelegateCommand LoginButtonEvent { get; }

        public AuthWindowViewModel()
        {
            var LoginButtonEvent = new DelegateCommand(LoginButton);
        }

        public void LoginButton()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44396/");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync("/token").Result;
            if (response.IsSuccessStatusCode)
            {
                var token = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine("User Found : " + token);
            }
            else
            {
                Console.WriteLine("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }
        }
    }
}