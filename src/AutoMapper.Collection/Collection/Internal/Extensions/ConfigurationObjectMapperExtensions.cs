using System;
using AutoMapper.EquivalencyExpression;
using AutoMapper.Mappers;

namespace AutoMapper.Collection.Internal.Extensions
{
    internal static class ConfigurationObjectMapperExtensions
    {
        internal static ICollectionMapper GetCollectionMapper(this IConfigurationObjectMapper mapper, Type sourceType, Type destinationType)
        {
            var typeMap = mapper.GetTypeMap(sourceType, destinationType);
            return typeMap == null ? null : mapper.GetCollectionMapper(typeMap);
        }

        internal static ICollectionMapper GetCollectionMapper(this IConfigurationObjectMapper mapper, TypeMap typeMap)
        {
            return mapper.ConfigurationProvider.GetCollectionMapper(typeMap);
        }

        internal static TypeMap GetTypeMap(this IConfigurationObjectMapper mapper, Type sourceType, Type destinationType)
        {
            return mapper.ConfigurationProvider.ResolveTypeMap(sourceType, destinationType);
        }
    }
}
