using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public EditTestWindowViewModel()
        {
            Test = new Test();

            for (int i = 0; i < 5; i++)
            {
                var ans = new List<TestAnswer>();
                for (int j = 0; j < 4; j++)
                {
                    ans.Add(new TestAnswer($"Answer #{j}", false));
                }
                Test.Questions.Add(new TestQuestion($"Question #{i}", ans));
            }
        }

        public void AddQuestionButton()
        {
            Test.Questions.Add(new TestQuestion());
        }
    }
}