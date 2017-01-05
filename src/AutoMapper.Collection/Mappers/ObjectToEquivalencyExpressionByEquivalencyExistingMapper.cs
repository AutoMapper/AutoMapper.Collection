using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.EquivilencyExpression;
using static System.Linq.Expressions.Expression;

namespace AutoMapper.Mappers
{
    public class ObjectToEquivalencyExpressionByEquivalencyExistingMapper : IConfigurationObjectMapper
    {
        public IConfigurationProvider ConfigurationProvider { get; set; }

        public static Expression<Func<TDestination, bool>> Map<TSource, TDestination>(TSource source, IEquivilentExpression<TSource, TDestination> toSourceExpression)
        {
            return toSourceExpression.ToSingleSourceExpression(source);
        }

        private static readonly MethodInfo MapMethodInfo = typeof(ObjectToEquivalencyExpressionByEquivalencyExistingMapper).GetRuntimeMethods().First(_ => _.IsStatic);
        
        public bool IsMatch(TypePair typePair)
        {
            var destExpressArgType = typePair.DestinationType.GetSinglePredicateExpressionArgumentType();
            if (destExpressArgType == null)
                return false;
            return this.GetEquivilentExpression(typePair.SourceType, destExpressArgType) != null;
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var destExpressArgType = destExpression.Type.GetSinglePredicateExpressionArgumentType();
            var toSourceExpression = this.GetEquivilentExpression(sourceExpression.Type, destExpressArgType);
            return Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpressArgType), sourceExpression, Constant(toSourceExpression));
        }
    }
}