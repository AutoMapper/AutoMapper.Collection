using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoMapper.Collection.Internal.Extensions
{
    public static class ExpressionExtensions
    {
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
}