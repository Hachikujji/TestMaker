using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using TestMaker.Common;

namespace TestMaker.UI.ViewModels
{
    public class MenuHubWindowViewModel : ViewModelBase
    {
        public DelegateCommand ReturnButtonEvent { get; }
        public DelegateCommand AddButtonEvent { get; }

        public MenuHubWindowViewModel(IRegionManager regionManager) : base(regionManager)
        {
            ReturnButtonEvent = new DelegateCommand(ReturnButton);
            AddButtonEvent = new DelegateCommand(AddButton);
        }

        public void ReturnButton()
        {
            RegionManager.RequestNavigate(RegionNames.ContentRegion, "AuthorizationWindow");
        }

        public void AddButton()
        {
            RegionManager.RequestNavigate(RegionNames.ContentRegion, "EditTestWindow");
        }
    }
}