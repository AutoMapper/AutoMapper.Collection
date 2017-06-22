using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Collection;
using AutoMapper.Collection.Internal.Extensions;
using AutoMapper.EquivalencyExpression;
using static System.Linq.Expressions.Expression;

namespace AutoMapper.Mappers
{
    public class EquivalentExpressionAddRemoveCollectionMapper : IConfigurationObjectMapper
    {
        private static readonly MethodInfo _mapMethodInfo = typeof(EquivalentExpressionAddRemoveCollectionMapper).GetRuntimeMethods().First(_ => _.IsStatic);
        private readonly CollectionMapper _collectionMapper = new CollectionMapper();
        public IConfigurationProvider ConfigurationProvider { get; set; }

        public bool IsMatch(TypePair typePair)
        {
            return typePair.SourceType.IsEnumerableType()
                   && typePair.DestinationType.IsCollectionType()
                   && this.GetCollectionMapper(TypeHelper.GetElementType(typePair.SourceType), TypeHelper.GetElementType(typePair.DestinationType)) != null;
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider,
            ProfileMap profileMap,
            PropertyMap propertyMap,
            Expression sourceExpression,
            Expression destExpression,
            Expression contextExpression)
        {
            var sourceType = TypeHelper.GetElementType(sourceExpression.Type);
            var destinationType = TypeHelper.GetElementType(destExpression.Type);

            var typeMap = this.GetTypeMap(sourceType, destinationType);

            if (typeMap.SourceType != sourceType)
            {
                throw new ArgumentException($"Source type '{sourceType.FullName}' is not mapped, use type '{typeMap.SourceType.FullName}' instead.");
            }
            if (typeMap.DestinationType != destinationType)
            {
                throw new ArgumentException($"Destination type '{destinationType.FullName}' is not mapped, use type '{typeMap.DestinationType.FullName}' instead.");
            }

            var collectionMapper = this.GetCollectionMapper(typeMap);

            var map = Call(null,
                _mapMethodInfo.MakeGenericMethod(sourceExpression.Type, sourceType, destExpression.Type, destinationType),
                sourceExpression, destExpression, contextExpression, Constant(collectionMapper));
            var collectionMap = _collectionMapper.MapExpression(configurationProvider, profileMap, propertyMap, sourceExpression, destExpression, contextExpression);

            var notNull = NotEqual(destExpression, Constant(null));
            return Condition(notNull, map, collectionMap);
        }

        public static TDestination Map<TSource, TSourceItem, TDestination, TDestinationItem>(TSource source, TDestination destination, ResolutionContext context, ICollectionMapper<TSourceItem, TDestinationItem> equivalencyExpression)
            where TSource : IEnumerable<TSourceItem>
            where TDestination : class, ICollection<TDestinationItem>
        {
            return equivalencyExpression.Map(source, destination, context);
        }
    }
}
