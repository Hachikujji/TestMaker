using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using TestMaker.Database.Entities;

namespace TestMaker.UI.ViewModels
{
    public class EditTestWindowViewModel : BindableBase
    {
        private Test _test;

        public Test Test
        {
            get { return _test; }
            set { SetProperty(ref _test, value); }
        }
        private int _attempts;

        public int Attempts
        {
            get { return _attempts; }
            set { SetProperty(ref _attempts, value); }
        }
        private string _testName;

        public string TestName
        {
            get { return _testName; }
            set { SetProperty(ref _testName, value); }
        }

        private TestQuestion _testQuestion;

        public TestQuestion TestQuestion
        {
            get { return _testQuestion; }
            set { SetProperty(ref _testQuestion, value); }
        }

        public DelegateCommand AddQuestionButtonEvent { get; }
        public DelegateCommand<object> AddAnswerButtonEvent { get; }
        public DelegateCommand<object> RemoveQuestionButtonEvent { get; }
        public DelegateCommand<object> RemoveAnswerButtonEvent { get; }
        public DelegateCommand<object> RemoveAnswerEnterButtonEvent { get; }
        public DelegateCommand CreateTestButtonEvent { get; }
        public ObservableCollection<int> NumbersOfAttempts { get; set; }

        public EditTestWindowViewModel()
        {
            AddQuestionButtonEvent = new DelegateCommand(AddQuestionButton);
            AddAnswerButtonEvent = new DelegateCommand<object>(AddAnswerButton);
            RemoveQuestionButtonEvent = new DelegateCommand<object>(RemoveQuestionButton);
            RemoveAnswerButtonEvent = new DelegateCommand<object>(RemoveAnswerButton);
            RemoveAnswerEnterButtonEvent = new DelegateCommand<object>(RemoveAnswerEnterButton);
            CreateTestButtonEvent = new DelegateCommand(CreateTestButton);
            Test = new Test("New test", new User(), 1, new ObservableCollection<TestQuestion>());
            NumbersOfAttempts = new ObservableCollection<int>();
            for (int i = 1; i < 6; i++)
                NumbersOfAttempts.Add(i);
            Test.Questions.Add(new TestQuestion($"New question", new ObservableCollection<TestAnswer>()));
        }

        public void AddQuestionButton()
        {
            Test.Questions.Add(new TestQuestion($"New Question", new ObservableCollection<TestAnswer>()));
        }

        public void AddAnswerButton(object testQuestion)
        {
            var question = (testQuestion as TestQuestion);
            question.Answers.Add(new TestAnswer("New answer", false));
        }

        public void RemoveQuestionButton(object testQuestion)
        {
            var question = (testQuestion as TestQuestion);
            Test.Questions.Remove(question);
        }

        public void RemoveAnswerButton(object testAnswer)
        {
            var Answer = (testAnswer as TestAnswer);
            TestQuestion.Answers.Remove(Answer);
        }

        public void RemoveAnswerEnterButton(object testQuestion)
        {
            var Question = (testQuestion as TestQuestion);
            TestQuestion = Question;
        }

        public void CreateTestButton()
        {
            Test.Attempts = Attempts;
            Test.Creator = new User();
            Test.Name = TestName;
            var a = Test;
            Debug.WriteLine(a);
        }
    }
}