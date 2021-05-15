using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using TestMaker.Common;
using TestMaker.UI.Views;

namespace TestMaker.UI
{
    public class UIModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(EditTestWindow));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<AuthorizationWindow>();
            containerRegistry.RegisterForNavigation<MenuHubWindow>();
            containerRegistry.RegisterForNavigation<EditTestWindow>();
        }
    }
}