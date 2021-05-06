using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using TestMaker.Authorization.Views;

namespace TestMaker.Authorization
{
    public class AuthorizationModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RequestNavigate("Authorization", nameof(Views.AuthorizationWindow));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<AuthorizationWindow>();
        }
    }
}