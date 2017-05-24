using InkCards.Services.Navigation;
using InkCards.Services.Storage;
using InkCards.Services.Testing;
using Microsoft.Practices.Unity;

namespace InkCards.Services.Bootstrap
{
    class UnityConfig
    {
        public void ConfigureContainer(IUnityContainer container, App app)
        {
            container.RegisterType<INavigationService, NavigationService>(new InjectionConstructor(new InjectionParameter(app.Frame)));

            container.RegisterType<ICardStorageService, FolderBasedCardStorageService>(new ContainerControlledLifetimeManager());
            container.RegisterType<ICardImpressionsStorageService, SqliteCardImpressionStorageService>();
            container.RegisterType<IPreferencesService, LocalDataPreferencesService>();

            container.RegisterType<IFlashcardSessionTestingService, FlashcardSessionTestingService>();

            container.RegisterType<ICardOrderingService, CardOrderingService>();

            container.RegisterType<IFirstRunService, FirstRunService>();
        }
    }
}
