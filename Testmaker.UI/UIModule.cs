using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using TestMaker.Stuff;
using TestMaker.UI.Services;
using TestMaker.UI.Views;

namespace TestMaker.UI
{
    public class UIModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RequestNavigate(StaticProperties.ContentRegion, nameof(AuthorizationWindow));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<AuthorizationWindow>();
            containerRegistry.RegisterForNavigation<MenuHubWindow>();
            containerRegistry.RegisterForNavigation<EditTestWindow>();
            containerRegistry.RegisterForNavigation<UserTestsWindow>();
            containerRegistry.RegisterForNavigation<AllowedTestsWindow>();
            containerRegistry.RegisterForNavigation<TestResultsWindow>();
            containerRegistry.RegisterForNavigation<CompletionTestWindow>();
            containerRegistry.RegisterForNavigation<UserTestResultsWindow>();
            containerRegistry.RegisterForNavigation<PreviewRightTestAnswersWindow>();
            containerRegistry.RegisterSingleton<ITokenHandler, TokenHandler>();
        }
    }
}