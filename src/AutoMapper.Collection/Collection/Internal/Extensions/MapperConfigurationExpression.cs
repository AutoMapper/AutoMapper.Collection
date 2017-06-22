using System.Linq;
using AutoMapper.Mappers;

namespace AutoMapper.Collection.Internal.Extensions
{
    internal static class MapperConfigurationExpressionExtensions
    {
        internal static void InsertBefore<TObjectMapper>(this IMapperConfigurationExpression cfg, params IConfigurationObjectMapper[] adds)
            where TObjectMapper : IObjectMapper
        {
            var mappers = cfg.Mappers;
            var targetMapper = mappers.FirstOrDefault(om => om is TObjectMapper);
            var index = targetMapper == null ? 0 : mappers.IndexOf(targetMapper);
            foreach (var mapper in adds.Reverse())
                mappers.Insert(index, mapper);
            cfg.Advanced.BeforeSeal(c =>
            {
                foreach (var configurationObjectMapper in adds)
                    configurationObjectMapper.ConfigurationProvider = c;

                CollectionMapperCache.CreateCollectionMappers(c);
                GeneratePropertyMapsCache.CreatePropertyMaps(c);
            });
        }
    }
}
