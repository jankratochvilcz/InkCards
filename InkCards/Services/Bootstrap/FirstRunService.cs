using InkCards.Services.Storage;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace InkCards.Services.Bootstrap
{
    public class FirstRunService : IFirstRunService
    {
        private const string CollectionsFolderName = "Collections";
        private const string AssetsFolderName = "Assets";
        private const string InitialDataFolderName = "InitialData";

        readonly IPreferencesService preferencesService;

        public FirstRunService(
            IPreferencesService preferencesService)
        {
            this.preferencesService = preferencesService;
        }

        public async Task InitializeIfFirstRun()
        {
            if (!this.preferencesService.IsFirstRun) return;
            this.preferencesService.IsFirstRun = false;

            var firstRunCollectionsFolder = await (await (await 
                Package.Current.InstalledLocation.GetFolderAsync(AssetsFolderName))
                .GetFolderAsync(InitialDataFolderName))
                .GetFolderAsync(CollectionsFolderName);

            var collectionsFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(CollectionsFolderName, CreationCollisionOption.OpenIfExists);

            foreach (var sourceFolder in await firstRunCollectionsFolder.GetFoldersAsync())
            {
                var targetFolder = await collectionsFolder.CreateFolderAsync(sourceFolder.Name, CreationCollisionOption.OpenIfExists);
                foreach (var sourceFile in await sourceFolder.GetFilesAsync())
                    await sourceFile.CopyAsync(targetFolder);
            }
        }
    }
}
