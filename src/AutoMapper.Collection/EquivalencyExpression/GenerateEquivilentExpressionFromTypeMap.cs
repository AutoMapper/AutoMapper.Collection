using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoMapper.EquivalencyExpression
{
    internal class GenerateEquivalentExpressionFromTypeMap
    {
        private static readonly ConcurrentDictionary<TypeMap, GenerateEquivalentExpressionFromTypeMap> _EquivalentExpressionses = new ConcurrentDictionary<TypeMap, GenerateEquivalentExpressionFromTypeMap>();
        internal static Expression GetExpression(TypeMap typeMap, object value)
        {
            return _EquivalentExpressionses.GetOrAdd(typeMap, t => new GenerateEquivalentExpressionFromTypeMap(t))
                .CreateEquivalentExpression(value);
        }

        private readonly TypeMap _typeMap;

        private GenerateEquivalentExpressionFromTypeMap(TypeMap typeMap)
        {
            _typeMap = typeMap;
        }

        private Expression CreateEquivalentExpression(object value)
        {
            var express = value as LambdaExpression;
            var destExpr = Expression.Parameter(_typeMap.SourceType, express.Parameters[0].Name);

            var result = new CustomExpressionVisitor(destExpr, _typeMap.GetPropertyMaps()).Visit(express.Body);

            return Expression.Lambda(result, destExpr);
        }
    }
}