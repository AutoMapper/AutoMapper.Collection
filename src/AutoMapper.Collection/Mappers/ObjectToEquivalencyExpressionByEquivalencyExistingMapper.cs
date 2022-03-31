using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.EquivalencyExpression;
using AutoMapper.Internal;
using AutoMapper.Internal.Mappers;
using static System.Linq.Expressions.Expression;

namespace AutoMapper.Mappers
{
    public class ObjectToEquivalencyExpressionByEquivalencyExistingMapper : IObjectMapper
    {
        internal IConfigurationProvider Configuration { get; set; }

        public static Expression<Func<TDestination, bool>> Map<TSource, TDestination>(TSource source, IEquivalentComparer<TSource, TDestination> toSourceExpression)
        {
            return toSourceExpression.ToSingleSourceExpression(source);
        }

        private static readonly MethodInfo MapMethodInfo = typeof(ObjectToEquivalencyExpressionByEquivalencyExistingMapper).GetRuntimeMethods().First(_ => _.IsStatic);
        
        public bool IsMatch(TypePair typePair)
        {
            var destExpressArgType = typePair.DestinationType.GetSinglePredicateExpressionArgumentType();
            if (destExpressArgType == null)
                return false;
            return this.GetEquivalentExpression(typePair.SourceType, destExpressArgType, Configuration) != null;
        }

        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, MemberMap memberMap,
            Expression sourceExpression, Expression destExpression)
        {
            var destExpressArgType = destExpression.Type.GetSinglePredicateExpressionArgumentType();
            var toSourceExpression = this.GetEquivalentExpression(sourceExpression.Type, destExpressArgType, configurationProvider);
            return Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpressArgType), sourceExpression, Constant(toSourceExpression));
        }
    }
}