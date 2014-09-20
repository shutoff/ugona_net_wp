using JetImageLoader;
using JetImageLoader.Cache;
using JetImageLoader.Cache.Memory;
using JetImageLoader.Cache.Memory.CacheImpl;
using JetImageLoader.Cache.Storage;
using JetImageLoader.Cache.Storage.CacheFileNameGenerators;
using JetImageLoader.Cache.Storage.CacheImpl;
using JetImageLoader.Network;
using System.IO;
using System.IO.IsolatedStorage;

namespace ugona_net
{
public class ImageLoader : BaseJetImageLoaderConverter
{
    public static readonly BaseMemoryCache<string, Stream> MemoryCacheImpl = new WeakMemoryCache<string, Stream>();
    public static readonly BaseStorageCache StorageCacheImpl = 
        new LimitedStorageCache(IsolatedStorageFile.GetUserStoreForApplication(), 
            "\\image_cache", new SHA1CacheFileNameGenerator(), 1024 * 1024 * 10);

    protected override JetImageLoaderConfig GetJetImageLoaderConfig()
    {

        return new JetImageLoaderConfig.Builder
        {
            IsLogEnabled     = true,
            CacheMode        = CacheMode.MemoryAndStorageCache,
            DownloaderImpl   = new HttpWebRequestDownloader(),
            MemoryCacheImpl  = MemoryCacheImpl,
            StorageCacheImpl = StorageCacheImpl
        }.Build();
    }
}
}
