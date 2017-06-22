using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Collection;
using AutoMapper.Collection.Internal.Extensions;
using static System.Linq.Expressions.Expression;

namespace AutoMapper.Mappers
{
    public class ObjectToEquivalencyExpressionByEquivalencyExistingMapper : IConfigurationObjectMapper
    {
        private static readonly MethodInfo _mapMethodInfo = typeof(ObjectToEquivalencyExpressionByEquivalencyExistingMapper).GetRuntimeMethods().First(_ => _.IsStatic);
        public IConfigurationProvider ConfigurationProvider { get; set; }

        public bool IsMatch(TypePair typePair)
        {
            var destExpressArgType = typePair.DestinationType.GetSinglePredicateExpressionArgumentType();
            if (destExpressArgType == null)
            {
                return false;
            }

            var typeMap = this.GetTypeMap(typePair.SourceType, destExpressArgType);
            return this.GetCollectionMapper(typeMap) != null;
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider,
            ProfileMap profileMap,
            PropertyMap propertyMap,
            Expression sourceExpression,
            Expression destExpression,
            Expression contextExpression)
        {
            var destExpressArgType = destExpression.Type.GetSinglePredicateExpressionArgumentType();
            var typeMap = this.GetTypeMap(sourceExpression.Type, destExpressArgType);
            var collectionMapper = this.GetCollectionMapper(typeMap);
            return Call(null, _mapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpressArgType), sourceExpression, Constant(collectionMapper));
        }

        public static Expression<Func<TDestination, bool>> Map<TSource, TDestination>(TSource source, ICollectionMapper<TSource, TDestination> collectionMapper)
        {
            var equalExpression = collectionMapper.EquivalentExpression;
            var expression = (LambdaExpression)new ParametersToConstantVisitor<TSource>(source).Visit(equalExpression);
            return Lambda<Func<TDestination, bool>>(expression.Body, equalExpression.Parameters[1]);
        }

        private class ParametersToConstantVisitor<T> : ExpressionVisitor
        {
            private readonly T _value;

            public ParametersToConstantVisitor(T value)
            {
                _value = value;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Member is PropertyInfo && node.Member.DeclaringType.GetTypeInfo().IsAssignableFrom(typeof(T).GetTypeInfo()))
                {
                    var memberExpression = Constant(node.Member.GetMemberValue(_value));
                    return memberExpression;
                }

                return base.VisitMember(node);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node;
            }
        }
    }
}
