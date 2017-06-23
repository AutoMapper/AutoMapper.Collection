using System;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Collection;

namespace AutoMapper.EquivalencyExpression
{
    internal class EquivalentExpression : IEquivalentComparer
    {
        internal static IEquivalentComparer BadValue { get; private set; }

        static EquivalentExpression()
        {
            BadValue = new EquivalentExpression();
        }

        public int GetHashCode(object obj)
        {
            throw new Exception("How'd you get here");
        }
    }

    internal class EquivalentExpression<TSource,TDestination> : IEquivalentComparer<TSource, TDestination>
        where TSource : class 
        where TDestination : class
    {
        private readonly Expression<Func<TSource, TDestination, bool>> _equivalentExpression;
        private readonly Func<TSource, TDestination, bool> _equivalentFunc;
        private readonly Func<TSource, int> _sourceHashCodeFunc;
        private readonly Func<TDestination, int> _destinationHashCodeFunc;

        public EquivalentExpression(Expression<Func<TSource,TDestination,bool>> equivalentExpression)
        {
            _equivalentExpression = equivalentExpression;
            _equivalentFunc = _equivalentExpression.Compile();

            var sourceParameter = equivalentExpression.Parameters[0];
            var destinationParameter = equivalentExpression.Parameters[1];

            var members = MemberExpressionExpando.Expand(sourceParameter, destinationParameter, equivalentExpression);
            
            _sourceHashCodeFunc = members.Item1.GetHashCodeExpression<TSource>(sourceParameter).Compile();
            _destinationHashCodeFunc = members.Item2.GetHashCodeExpression<TDestination>(destinationParameter).Compile();
        }

        public bool IsEquivalent(TSource source, TDestination destination)
        {
            return _equivalentFunc(source, destination);
        }

        public Expression<Func<TDestination, bool>> ToSingleSourceExpression(TSource source)
        {
            if (source == null)
                throw new Exception("Invalid somehow");

            var expression = new ParametersToConstantVisitor<TSource>(source).Visit(_equivalentExpression) as LambdaExpression;
            return Expression.Lambda<Func<TDestination, bool>>(expression.Body, _equivalentExpression.Parameters[1]);
        }

        public int GetHashCode(object obj)
        {
            if (obj is TSource)
                return _sourceHashCodeFunc(obj as TSource);
            if (obj is TDestination)
                return _destinationHashCodeFunc(obj as TDestination);
            return default(int);
        }
    }

    internal class ParametersToConstantVisitor<T> : ExpressionVisitor
    {
        private readonly T _value;

        public ParametersToConstantVisitor(T value)
        {
            _value = value;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member is PropertyInfo && node.Member.DeclaringType.GetTypeInfo().IsAssignableFrom(typeof(T).GetTypeInfo()))
            {
                var memberExpression = Expression.Constant(node.Member.GetMemberValue(_value));
                return memberExpression;
            }
            return base.VisitMember(node);
        }
    }
}