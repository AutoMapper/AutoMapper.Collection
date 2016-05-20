using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoMapper.EquivilencyExpression
{
    internal class GenerateEquivilentExpressionFromTypeMap
    {
        private static readonly ConcurrentDictionary<TypeMap, GenerateEquivilentExpressionFromTypeMap> _equivilentExpressionses = new ConcurrentDictionary<TypeMap, GenerateEquivilentExpressionFromTypeMap>();
        internal static Expression GetExpression(TypeMap typeMap, object value)
        {
            return _equivilentExpressionses.GetOrAdd(typeMap, t => new GenerateEquivilentExpressionFromTypeMap(t))
                .CreateEquivilentExpression(value);
        }

        private readonly TypeMap _typeMap;

        private GenerateEquivilentExpressionFromTypeMap(TypeMap typeMap)
        {
            _typeMap = typeMap;
        }

        private Expression CreateEquivilentExpression(object value)
        {
            var express = value as LambdaExpression;
            var destExpr = Expression.Parameter(_typeMap.SourceType, express.Parameters[0].Name);

            var result = new CustomExpressionVisitor(destExpr, _typeMap.GetPropertyMaps()).Visit(express.Body);

            return Expression.Lambda(result, destExpr);
        }
    }
}