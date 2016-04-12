using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.EquivilencyExpression
{
    public static class ExpressionExtentions
    {
        private static readonly ConcurrentDictionary<Type, Type> _singleParameterTypeDictionary = new ConcurrentDictionary<Type, Type>();

        public static Type GetSinglePredicateExpressionArgumentType(this Type type)
        {
            return _singleParameterTypeDictionary.GetOrAdd(type, t =>
            {
                var isExpression = typeof(Expression).IsAssignableFrom(t);
                if (!isExpression)
                    return null;

                var expressionOf = t.GetGenericArguments().First();
                var isFunction = expressionOf.GetGenericTypeDefinition() == typeof(Func<,>);
                if (!isFunction)
                    return null;

                var isPredicate = expressionOf.GetGenericArguments()[1] == typeof(bool);
                if (!isPredicate)
                    return null;

                var objType = expressionOf.GetGenericArguments().First();
                return objType;
            });
        }
    }
}