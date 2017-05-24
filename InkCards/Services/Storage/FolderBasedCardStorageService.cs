using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InkCards.Models.Cards;
using Windows.Storage;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace InkCards.Services.Storage
{
    public class FolderBasedCardStorageService : ICardStorageService
    {
        private const string CardCollectionFileName = "Collection.json";
        private const string CardNameRegexPattern = @"^(?<cardId>[\w\d\-]+).(?<type>(front|back)).gif$";
        private const string CollectionsFolderName = "Collections";

        private StorageFolder collectionsFolder;
        private readonly Regex cardNameRegex;

        private ConcurrentDictionary<Guid, Task> savesInProgress = new ConcurrentDictionary<Guid, Task>();

        public FolderBasedCardStorageService()
        {
            this.cardNameRegex = new Regex(CardNameRegexPattern);
        }

        public async Task<IEnumerable<InkCard>> GetCards(Guid cardCollectionId)
        {
            await this.EnsureInitialized();

            var collectionFolder = await this.collectionsFolder.CreateFolderAsync(cardCollectionId.ToString(), CreationCollisionOption.OpenIfExists);
            var cardFiles = await this.GetCardFiles(collectionFolder);
            var cards = cardFiles.Select(x => this.GetInkCard(cardCollectionId, x));

            return await Task.WhenAll(cards);
        }

        public async Task<IEnumerable<InkCard>> GetCards(IEnumerable<Guid> collectionIds)
            => (await Task.WhenAll(collectionIds.Select(this.GetCards)))
                .SelectMany(x => x);

        public async Task<InkCard> GetCard(Guid cardCollectionId, Guid cardId)
        {
            await this.EnsureInitialized();

            var collectionFolder = await this.collectionsFolder.CreateFolderAsync(cardCollectionId.ToString(), CreationCollisionOption.OpenIfExists);
            var cardFile = (await this.GetCardFiles(collectionFolder)).FirstOrDefault(x => x.Key == cardId);
            if (cardFile == null) return null;

            return await this.GetInkCard(cardCollectionId, cardFile);
        }

        public async Task<CardCollection> GetCollection(Guid collectionId)
        {
            await this.EnsureInitialized();

            var collectionFolder = await this.GetCollectionFolder(collectionId);
            var collectionFile = await collectionFolder.TryGetItemAsync(FolderBasedCardStorageService.CardCollectionFileName);

            if (collectionFile == null) return null;

            return await this.GetCollectionFromFile((IStorageFile)collectionFile);
        }

        public async Task<IEnumerable<CardCollection>> GetCollections(IEnumerable<Guid> collectionIds)
            => await Task.WhenAll(collectionIds.Select(this.GetCollection));

        public async Task<IEnumerable<CardCollection>> GetCollections()
        {
            await this.EnsureInitialized();

            var collectionFolders = await Task.WhenAll((await this.collectionsFolder.GetFoldersAsync())
                .Select(async x => new
                {
                    Folder = x,
                    FileCount = (await x.GetFilesAsync()).Count
                }));

            var nonEmptyCollectionFolders = collectionFolders
                .Where(x => x.FileCount > 1)
                .Select(x => x.Folder);

            var collectionFiles = (await Task.WhenAll(nonEmptyCollectionFolders
                .Select(x => x.TryGetItemAsync(FolderBasedCardStorageService.CardCollectionFileName).AsTask())))
                .Where(x => x != null)
                .Cast<IStorageFile>();

            return await Task.WhenAll(collectionFiles.Select(this.GetCollectionFromFile));
        }
        public async Task SaveCollection(CardCollection collection)
        {
            await this.EnsureInitialized();

            var collectionFolder = await this.GetCollectionFolder(collection.Id);
            var collectionFile = await collectionFolder.CreateFileAsync(FolderBasedCardStorageService.CardCollectionFileName, CreationCollisionOption.ReplaceExisting);

            var serializedCardCollection = JsonConvert.SerializeObject(collection);
            await FileIO.WriteTextAsync(collectionFile, serializedCardCollection);
        }

        public async Task DeleteCollection(Guid collectionId)
        {
            await this.EnsureInitialized();

            var collectionFolder = await this.GetCollectionFolder(collectionId);
            await collectionFolder.DeleteAsync();
        }

        public async Task SaveCard(InkCard card)
        {
            try
            {
                await this.EnsureInitialized();

                if (savesInProgress.TryGetValue(card.CardId, out var saveInProgress))
                    await saveInProgress;

                var taskCompletionSource = new TaskCompletionSource<bool>();
                savesInProgress.TryAdd(card.CardId, taskCompletionSource.Task);

                var targetFolder = await this.GetCollectionFolder(card.CardCollectionId);

                var result = await Task.WhenAll(
                    this.SaveInkFile(card.CardFrontInk, targetFolder, this.GetCardFrontFileName(card)),
                    this.SaveInkFile(card.CardBackInk, targetFolder, this.GetCardBackFileName(card)));

                taskCompletionSource.SetResult(true);
                savesInProgress.TryRemove(card.CardId, out var _);

                card.CardFrontUri = new Uri(result[0].Path, UriKind.Absolute);
                card.CardBackUri = new Uri(result[1].Path, UriKind.Absolute);
            }
            catch (Exception ex)
            {

            }

        }

        public async Task LoadCard(InkCard card)
        {
            await this.EnsureInitialized();

            if (savesInProgress.TryGetValue(card.CardId, out var saveInProgress))
                await saveInProgress;

            var targetFolder = await this.GetCollectionFolder(card.CardCollectionId);

            var result = await Task.WhenAll(
                this.LoadInkFile(targetFolder, this.GetCardFrontFileName(card)),
                this.LoadInkFile(targetFolder, this.GetCardBackFileName(card)));

            card.CardFrontInk = result[0];
            card.CardBackInk = result[1];
        }

        public async Task DeleteCard(InkCard card)
        {
            await this.EnsureInitialized();

            if (savesInProgress.TryGetValue(card.CardId, out var saveInProgress))
                await saveInProgress;

            var folder = await this.GetCollectionFolder(card.CardCollectionId);

            var filesToDelete = (await Task.WhenAll(
                folder.TryGetItemAsync(this.GetCardFrontFileName(card)).AsTask(),
                folder.TryGetItemAsync(this.GetCardBackFileName(card)).AsTask()))
                .Where(x => x != null);

            await Task.WhenAll(filesToDelete.Select(x => x.DeleteAsync().AsTask()));
        }

        private string GetCardBackFileName(InkCard card) => $"{card.CardId}.back.gif";
        private string GetCardFrontFileName(InkCard card) => $"{card.CardId}.front.gif";

        private async Task<StorageFile> SaveInkFile(IRandomAccessStream inkStream, StorageFolder targetFolder, string fileName)
        {
            var frontFile = await targetFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            try
            {
                using (var readStream = inkStream)
                {
                    using (var readStreamInMemory = new MemoryStream())
                    {
                        readStreamInMemory.Capacity = (int)readStream.Size;
                        var buffer = readStreamInMemory.GetWindowsRuntimeBuffer();

                        await readStream
                            .GetInputStreamAt(0)
                            .ReadAsync(buffer, (uint)readStream.Size, InputStreamOptions.None);

                        using (var writeStream = await frontFile.OpenStreamForWriteAsync())
                        {
                            await writeStream.WriteAsync(buffer.ToArray(), 0, (int)buffer.Length);
                            await writeStream.FlushAsync();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return frontFile;
        }

        private Task<StorageFolder> GetCollectionFolder(Guid collectionId)
            => this.collectionsFolder.CreateFolderAsync(collectionId.ToString(), CreationCollisionOption.OpenIfExists).AsTask();

        private async Task<IRandomAccessStream> LoadInkFile(StorageFolder folder, string fileName)
        {
            var file = await folder.TryGetItemAsync(fileName);
            if (file == null) return null;

            return await ((StorageFile)file).OpenAsync(FileAccessMode.Read).AsTask();
        }

        private async Task EnsureInitialized()
        {
            if (this.collectionsFolder != null) return;
            this.collectionsFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(CollectionsFolderName, CreationCollisionOption.OpenIfExists);
        }

        private async Task<CardCollection> GetCollectionFromFile(IStorageFile collectionFile)
        {
            var serializedCardCollection = await FileIO.ReadTextAsync(collectionFile);
            var deserializedCardCollection = JsonConvert.DeserializeObject<CardCollection>(serializedCardCollection);

            return deserializedCardCollection;
        }

        private async Task<IEnumerable<IGrouping<Guid, CardFile>>> GetCardFiles(StorageFolder folder)
        {
            return (await folder.GetFilesAsync())
                .Where(x => Path.GetExtension(x.Path) == ".gif")
                .OrderBy(x => x.DateCreated)
                .Select(file =>
                {
                    var match = this.cardNameRegex.Match(file.Name);
                    return new CardFile
                    {
                        IsFront = match.Groups[1].Value == "front",
                        CardId = new Guid(match.Groups[2].Value),
                        File = file
                    };
                })
                .GroupBy(x => x.CardId);
        }

        private async Task<InkCard> GetInkCard(Guid cardCollectionId, IGrouping<Guid, CardFile> x)
        {
            var frontFile = x.FirstOrDefault(y => y.IsFront)?.File;
            var backFile = x.FirstOrDefault(y => !y.IsFront)?.File;

            return new InkCard
            {
                CardCollectionId = cardCollectionId,
                CardId = x.Key,
                CardFrontInk = await frontFile.OpenReadAsync(),
                CardBackInk = await backFile.OpenReadAsync(),
                CardFrontUri = new Uri(frontFile.Path, UriKind.Absolute),
                CardBackUri = new Uri(backFile.Path, UriKind.Absolute),
                Created = frontFile.DateCreated.DateTime
            };
        }

        private class CardFile
        {
            public Guid CardId { get; set; }

            public bool IsFront { get; set; }

            public StorageFile File { get; set; }
        }
    }
}
