using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    public class EditTestWindowViewModel : ViewModelBase
    {
        #region Private Fields

        private Test _test;
        private int _attempts;
        private string _testName;
        private TestQuestion _testQuestion;
        private readonly ITokenHandler _tokenHandler;

        #endregion Private Fields

        #region Public Properties

        public Test Test
        {
            get { return _test; }
            set { SetProperty(ref _test, value); }
        }

        private bool _isTestEditing;

        public bool IsTestEditing
        {
            get { return _isTestEditing; }
            set { SetProperty(ref _isTestEditing, value); }
        }

        public int Attempts
        {
            get { return _attempts; }
            set { SetProperty(ref _attempts, value); }
        }

        public string TestName
        {
            get { return _testName; }
            set { SetProperty(ref _testName, value); }
        }

        private string _createTestButtonName;

        public string CreateTestButtonName
        {
            get { return _createTestButtonName; }
            set { SetProperty(ref _createTestButtonName, value); }
        }

        public TestQuestion TestQuestion
        {
            get { return _testQuestion; }
            set { SetProperty(ref _testQuestion, value); }
        }

        public DelegateCommand AddQuestionButtonCommand { get; }
        public DelegateCommand<object> AddAnswerButtonCommand { get; }
        public DelegateCommand<object> RemoveQuestionButtonCommand { get; }
        public DelegateCommand<object> RemoveAnswerButtonCommand { get; }
        public DelegateCommand<object> RemoveAnswerEnterButtonCommand { get; }
        public DelegateCommand CreateTestButtonCommand { get; }
        public DelegateCommand ReturnButtonCommand { get; }
        public ObservableCollection<int> NumbersOfAttempts { get; set; }

        #endregion Public Properties

        #region Public Constructors

        public EditTestWindowViewModel(IRegionManager regionManager, ITokenHandler tokenHandler) : base(regionManager)
        {
            _tokenHandler = tokenHandler;
            AddQuestionButtonCommand = new DelegateCommand(AddQuestionButton);
            AddAnswerButtonCommand = new DelegateCommand<object>(AddAnswerButton);
            RemoveQuestionButtonCommand = new DelegateCommand<object>(RemoveQuestionButton);
            RemoveAnswerButtonCommand = new DelegateCommand<object>(RemoveAnswerButton);
            ReturnButtonCommand = new DelegateCommand(ReturnButton);
            RemoveAnswerEnterButtonCommand = new DelegateCommand<object>(RemoveAnswerEnterButton);
            CreateTestButtonCommand = new DelegateCommand(async () => await CreateTestButton());
            Test = new Test("New test", StaticProperties.CurrentUserResponseHeader.Username, 0, new ObservableCollection<TestQuestion>());
            NumbersOfAttempts = new ObservableCollection<int>();
            for (int i = 1; i < 6; i++)
                NumbersOfAttempts.Add(i);
            Test.Questions.Add(new TestQuestion($"New question", new ObservableCollection<TestAnswer>()));
            IsTestEditing = true;
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Add question button event
        /// </summary>
        public void AddQuestionButton()
        {
            Test.Questions.Add(new TestQuestion($"New question", new ObservableCollection<TestAnswer>()));
        }

        /// <summary>
        /// Add answer button event
        /// </summary>
        /// <param name="testQuestion">Test question</param>
        public void AddAnswerButton(object testQuestion)
        {
            var question = (testQuestion as TestQuestion);
            question.Answers.Add(new TestAnswer("New answer", false));
        }

        /// <summary>
        /// Remove question button event
        /// </summary>
        /// <param name="testQuestion">Test Question</param>
        public void RemoveQuestionButton(object testQuestion)
        {
            var question = (testQuestion as TestQuestion);
            Test.Questions.Remove(question);
        }

        /// <summary>
        /// Remove test answer button event, executed after RemoveAnswerEnterButton()
        /// </summary>
        /// <param name="testAnswer">Test answer</param>
        public void RemoveAnswerButton(object testAnswer)
        {
            var Answer = (testAnswer as TestAnswer);
            TestQuestion.Answers.Remove(Answer);
        }

        /// <summary>
        /// Remove answer button event, receiving TestQuestion first
        /// </summary>
        /// <param name="testQuestion"></param>
        public void RemoveAnswerEnterButton(object testQuestion)
        {
            var Question = (testQuestion as TestQuestion);
            TestQuestion = Question;
        }

        /// <summary>
        /// Create test button event, validate test
        /// </summary>
        /// <returns>Task</returns>
        public async Task CreateTestButton()
        {
            if (Test.Attempts == 0)
            {
                MessageBox.Show("Attempt count error");
                return;
            }
            if (string.IsNullOrWhiteSpace(Test.Name))
            {
                MessageBox.Show("Test name error");
                return;
            }
            if (Test.Questions.Count == 0)
            {
                MessageBox.Show("Question count error");
                return;
            }
            foreach (var question in Test.Questions)
            {
                if (string.IsNullOrWhiteSpace(question.Question))
                {
                    MessageBox.Show("One of question name is empty");
                    return;
                }
                if (question.Answers.Count < 2)
                {
                    MessageBox.Show($"Answer count error in {question.Question}");
                    return;
                }
                int count = 0;
                foreach (var answer in question.Answers)
                {
                    if (string.IsNullOrWhiteSpace(answer.Answer))
                    {
                        MessageBox.Show($"Answer name error in {question.Question}");
                        return;
                    }
                    if (answer.IsRight)
                        count++;
                }
                if (count == 0)
                {
                    MessageBox.Show($"Right answers count error in {question.Question}");
                    return;
                }
            }
            if (IsTestEditing)
            {
                if (await TrySendTest())
                {
                }
                else
                {
                    if (await _tokenHandler.TryRefreshTokenAsync())
                    {
                        await TrySendTest();
                    }
                    else
                    {
                        MessageBox.Show("Your token is expired.");
                        RegionManager.RequestNavigate(StaticProperties.ContentRegion, "AuthorizationWindow");
                    }
                }
            }
            else
            {
                if (await TryUpdateTest())
                {
                }
                else
                {
                    if (await _tokenHandler.TryRefreshTokenAsync())
                    {
                        await TryUpdateTest();
                    }
                    else
                    {
                        MessageBox.Show("Your token is expired.");
                        RegionManager.RequestNavigate(StaticProperties.ContentRegion, "AuthorizationWindow");
                    }
                }
            }
        }

        /// <summary>
        /// navigated to UserControl event, if navigation context!=null => not new test => edit test that already exists
        /// </summary>
        /// <param name="navigationContext">key=value vars</param>
        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            int editTestId = 0;
            string editTestIdString;
            if (navigationContext.Parameters.Count != 0)
            {
                editTestIdString = navigationContext.Parameters["TestId"].ToString();
                if (!string.IsNullOrWhiteSpace(editTestIdString))
                    int.TryParse(editTestIdString, out editTestId);
                if (editTestId > 0)
                    if (!await TryGetTest(editTestId))
                    {
                        if (await _tokenHandler.TryRefreshTokenAsync())
                        {
                            await TryGetTest(editTestId);
                        }
                        else
                        {
                            MessageBox.Show("Your token is expired.");
                            RegionManager.RequestNavigate(StaticProperties.ContentRegion, "AuthorizationWindow");
                        }
                    }
            }
            else
            {
                Test = new Test("New test", StaticProperties.CurrentUserResponseHeader.Username, 0, new ObservableCollection<TestQuestion>());
                IsTestEditing = true;
                CreateTestButtonName = "Create Test";
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// return button event
        /// </summary>
        public void ReturnButton()
        {
            RegionManager.RequestNavigate(StaticProperties.ContentRegion, "MenuHubWindow");
        }

        /// <summary>
        /// try send test
        /// </summary>
        /// <returns>true if request is success,else returns false</returns>
        private async Task<bool> TrySendTest()
        {
            var json = JsonConvert.SerializeObject(Test);
            var response = await StaticProperties.Client.PostAsync("/test/addTest", new StringContent(json, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                RegionManager.RequestNavigate(StaticProperties.ContentRegion, "MenuHubWindow");
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// try update test
        /// </summary>
        /// <returns>true if request is success,else returns false</returns>
        private async Task<bool> TryUpdateTest()
        {
            var json = JsonConvert.SerializeObject(Test);
            var response = await StaticProperties.Client.PostAsync("/test/updateTest", new StringContent(json, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                RegionManager.RequestNavigate(StaticProperties.ContentRegion, "MenuHubWindow");
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// try get test by id
        /// </summary>
        /// <returns>true if request is success,else returns false</returns>
        private async Task<bool> TryGetTest(int testId)
        {
            var request = JsonConvert.SerializeObject(testId);
            var response = await StaticProperties.Client.PostAsync("/test/getTest", new StringContent(request, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var testJson = await response.Content.ReadAsStringAsync();
                    var test = JsonConvert.DeserializeObject<Test>(testJson);
                    Test = test;
                    IsTestEditing = false;
                    CreateTestButtonName = "Update test";
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error when tried to get test: {e}");
                }
                return true;
            }
            else
                return false;
        }

        #endregion Private Methods
    }
}