using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Collection;

namespace AutoMapper.EquivalencyExpression
{
    internal static class ExpressionExtentions
    {
        private static readonly ConcurrentDictionary<Type, Type> _singleParameterTypeDictionary = new ConcurrentDictionary<Type, Type>();

        public static Type GetSinglePredicateExpressionArgumentType(this Type type)
        {
            return _singleParameterTypeDictionary.GetOrAdd(type, t =>
            {
                var isExpression = typeof (Expression).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo());
                if (!isExpression)
                    return null;

                var expressionOf = t.GetTypeInfo().GenericTypeArguments.First();
                var isFunction = expressionOf.GetGenericTypeDefinition() == typeof (Func<,>);
                if (!isFunction)
                    return null;

                var isPredicate = expressionOf.GetTypeInfo().GenericTypeArguments[1] == typeof (bool);
                if (!isPredicate)
                    return null;

                var objType = expressionOf.GetTypeInfo().GenericTypeArguments.First();
                return CacheAndReturnType(type, objType);
            });
        }

        private static Type CacheAndReturnType(Type type, Type objType)
        {
            _singleParameterTypeDictionary.AddOrUpdate(type, objType, (t,t2) => objType);
            return objType;
        }
        
        public static Expression<Func<T, int>> GetHashCodeExpression<T>(this List<Expression> members, ParameterExpression sourceParam)
        {
            var hashMultiply = Expression.Constant(397L);

            var hashVariable = Expression.Variable(typeof(long), "hashCode");
            var returnTarget = Expression.Label(typeof(int));
            var returnExpression = Expression.Return(returnTarget, Expression.Convert(hashVariable, typeof(int)), typeof(int));
            var returnLabel = Expression.Label(returnTarget, Expression.Constant(-1));

            var getHashCodeMethod = typeof(T).GetDeclaredMethod(nameof(GetHashCode));

            var expressions = new List<Expression>();
            foreach (var member in members)
            {
                var callGetHashCode = Expression.Call(member, getHashCodeMethod);
                var convertHashCodeToInt64 = Expression.Convert(callGetHashCode, typeof(long));
                if (expressions.Count == 0)
                {
                    expressions.Add(Expression.Assign(hashVariable, convertHashCodeToInt64));
                }
                else
                {
                    var oldHashMultiplied = Expression.Multiply(hashVariable, hashMultiply);
                    var xOrHash = Expression.ExclusiveOr(oldHashMultiplied, convertHashCodeToInt64);
                    expressions.Add(Expression.Assign(hashVariable, xOrHash));
                }
            }

            expressions.Add(returnExpression);
            expressions.Add(returnLabel);

            var resutltBlock = Expression.Block(new[] { hashVariable }, expressions);

            return Expression.Lambda<Func<T, int>>(resutltBlock, sourceParam);
        }
    }

    internal class MemberExpressionExpando : ExpressionVisitor
    {
        private readonly List<Expression> _destinationMembers = new List<Expression>();
        private readonly ParameterExpression _destinationParameter;
        private readonly List<Expression> _sourceMembers = new List<Expression>();
        private readonly ParameterExpression _sourceParameter;

        internal MemberExpressionExpando(ParameterExpression sourceParameter, ParameterExpression destinationParameter)
        {
            _sourceParameter = sourceParameter;
            _destinationParameter = destinationParameter;
        }

        internal static Tuple<List<Expression>, List<Expression>> Expand(ParameterExpression sourceParameter, ParameterExpression destinationParameter, Expression expression)
        {
            var visitor = new MemberExpressionExpando(sourceParameter, destinationParameter);
            visitor.Visit(expression);
            return Tuple.Create(visitor._sourceMembers, visitor._destinationMembers);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return base.VisitBinary(node);
            }

            Error();
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression == _sourceParameter)
            {
                _sourceMembers.Add(node);
            }
            else if (node.Expression == _destinationParameter)
            {
                _destinationMembers.Add(node);
            }

            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            Error();
            return node;
        }

        private void Error()
        {
            _sourceMembers.Clear();
            _destinationMembers.Clear();
        }
    }
}