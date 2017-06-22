using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutoMapper.Collection.Internal
{
    internal static class CollectionMapperCache
    {
        private static ConcurrentDictionary<TypePair, ICollectionMapper> _collectionMapperCache = new ConcurrentDictionary<TypePair, ICollectionMapper>();
        private static readonly
            IDictionary<IConfigurationProvider, ConcurrentDictionary<TypePair, ICollectionMapper>> _collectionMappersDictionary =
                new Dictionary<IConfigurationProvider, ConcurrentDictionary<TypePair, ICollectionMapper>>();

        public static void AddOrUpdate(TypePair typePair, ICollectionMapper collectionMapper)
        {
            _collectionMapperCache.AddOrUpdate(typePair,
                collectionMapper,
                (type, old) => collectionMapper);
        }

        public static ConcurrentDictionary<TypePair, ICollectionMapper> Get(IConfigurationProvider configurationProvider)
        {
            ConcurrentDictionary<TypePair, ICollectionMapper> item;
            _collectionMappersDictionary.TryGetValue(configurationProvider, out item);
            return item;
        }

        internal static void CreateCollectionMappers(IConfigurationProvider configurationProvider)
        {
            _collectionMappersDictionary[configurationProvider] = _collectionMapperCache;
            _collectionMapperCache = new ConcurrentDictionary<TypePair, ICollectionMapper>();
        }
    }
}
