using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.EquivilencyExpression;

namespace AutoMapper.Mappers
{
    public class ObjectToEquivalencyExpressionByEquivalencyExistingMapper : IObjectMapper
    {
        public static Expression<Func<TDestination, bool>> Map<TSource, TDestination>(TSource source)
        {
            var toSourceExpression = EquivilentExpressions.GetEquivilentExpression(typeof(TSource),typeof(TDestination)) as IEquivilentExpression<TSource, TDestination>;
            return toSourceExpression.ToSingleSourceExpression(source);
        }

        private static readonly MethodInfo MapMethodInfo = typeof(ObjectToEquivalencyExpressionByEquivalencyExistingMapper).GetRuntimeMethods().First(_ => _.IsStatic);
        
        public bool IsMatch(TypePair typePair)
        {
            var destExpressArgType = typePair.DestinationType.GetSinglePredicateExpressionArgumentType();
            if (destExpressArgType == null)
                return false;
            var expression = EquivilentExpressions.GetEquivilentExpression(typePair.SourceType, destExpressArgType);
            return expression != null;
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var destExpressArgType = destExpression.Type.GetSinglePredicateExpressionArgumentType();
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpressArgType), sourceExpression);
        }
    }
}