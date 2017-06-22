using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Collection.Internal.Mapper;

namespace AutoMapper.Collection.Internal.Extensions
{
    internal static class ConfigurationProviderExtensions
    {
        internal static ICollectionMapper GetCollectionMapper(this IConfigurationProvider configurationProvider, TypeMap typeMap)
        {
            return CollectionMapperCache.Get(configurationProvider).GetOrAdd(typeMap.Types,
                tp =>
                    GeneratePropertyMapsCache.GetGeneratePropertyMaps(configurationProvider)
                                     .Select(_ => _
                                         .GeneratePropertyMaps(typeMap)
                                         .CreateCollectionMapper()
                                     )
                                     .FirstOrDefault(_ => _ != null));
        }

        private static ICollectionMapper CreateCollectionMapper(this IEnumerable<PropertyMap> propertyMaps)
        {
            var properties = propertyMaps as IList<PropertyMap> ?? propertyMaps.ToList();
            if (!properties.Any() || properties.Any(pm => pm.DestinationProperty.GetMemberType() != pm.SourceMember.GetMemberType()))
            {
                return null;
            }

            var typeMap = properties.First().TypeMap;
            var srcType = typeMap.SourceType;
            var destType = typeMap.DestinationType;
            var srcExpr = Expression.Parameter(srcType, "src");
            var destExpr = Expression.Parameter(destType, "dest");

            var equalExpr = properties.Select(pm => SourceEqualsDestinationExpression(pm, srcExpr, destExpr)).ToList();
            if (!equalExpr.Any())
            {
                return null;
            }

            var finalExpression = equalExpr.Skip(1).Aggregate(equalExpr.First(), Expression.And);

            var expr = Expression.Lambda(finalExpression, srcExpr, destExpr);

            var collectionExpressionType = typeof(EqualityCollectionExpression<,>).MakeGenericType(srcType, destType);
            var collectionExpression = Activator.CreateInstance(collectionExpressionType, expr);

            var collectionMapperType = typeof(ExpressionCollectionMapper<,>).MakeGenericType(srcType, destType);
            var collectionMapper = Activator.CreateInstance(collectionMapperType, collectionExpression) as ICollectionMapper;
            return collectionMapper;
        }

        private static BinaryExpression SourceEqualsDestinationExpression(PropertyMap propertyMap, Expression srcExpr, Expression destExpr)
        {
            var srcPropExpr = Expression.Property(srcExpr, propertyMap.SourceMember as PropertyInfo);
            var destPropExpr = Expression.Property(destExpr, propertyMap.DestinationProperty as PropertyInfo);
            return Expression.Equal(srcPropExpr, destPropExpr);
        }
    }
}
