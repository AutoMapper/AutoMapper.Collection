using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.EquivilencyExpression
{
    public static class ExpressionExtentions
    {
        private static readonly ConcurrentDictionary<Type, Type> _singleParameterTypeDictionary = new ConcurrentDictionary<Type, Type>();

        public static Type GetSinglePredicateExpressionArgumentType(this Type type)
        {
            if (_singleParameterTypeDictionary.ContainsKey(type))
                return _singleParameterTypeDictionary[type];

            var isExpression = typeof(Expression).IsAssignableFrom(type);
            if (!isExpression)
                return CacheAndReturnType(type, null);

            var expressionOf = type.GetGenericArguments().First();
            var isFunction = expressionOf.GetGenericTypeDefinition() == typeof(Func<,>);
            if (!isFunction)
                return CacheAndReturnType(type, null);

            var isPredicate = expressionOf.GetGenericArguments()[1] == typeof(bool);
            if (!isPredicate)
                return CacheAndReturnType(type, null);

            var objType = expressionOf.GetGenericArguments().First();
            return CacheAndReturnType(type, objType);
        }

        private static Type CacheAndReturnType(Type type, Type objType)
        {
            _singleParameterTypeDictionary.AddOrUpdate(type, objType, (t,t2) => objType);
            return objType;
        }
    }
}