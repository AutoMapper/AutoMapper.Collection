using System;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Collection;

namespace AutoMapper.EquivilencyExpression
{
    internal class EquivilentExpression : IEquivilentExpression
    {
        internal static IEquivilentExpression BadValue { get; private set; }

        static EquivilentExpression()
        {
            BadValue = new EquivilentExpression();
        }
    }

    internal class EquivilentExpression<TSource,TDestination> : IEquivilentExpression<TSource, TDestination>
        where TSource : class 
        where TDestination : class
    {
        private readonly Expression<Func<TSource, TDestination, bool>> _equivilentExpression;
        private readonly Func<TSource, TDestination, bool> _equivilentFunc;
        private readonly Action<TDestination, bool> _softDeleteAction;

        public EquivilentExpression(Expression<Func<TSource,TDestination,bool>> equivilentExpression, Action<TDestination, bool> softDeleteAction = null)
        {
            _equivilentExpression = equivilentExpression;
            _equivilentFunc = _equivilentExpression.Compile();
            _softDeleteAction = softDeleteAction;
        }

        public bool IsEquivlent(TSource source, TDestination destination)
        {
            return _equivilentFunc(source, destination);
        }

        public bool IsSoftDelete()
        {
            return _softDeleteAction != null;
        }

        public void SetSoftDeleteValue(TDestination detination, bool value)
        {
            _softDeleteAction(detination, value);
        }

        public Expression<Func<TDestination, bool>> ToSingleSourceExpression(TSource source)
        {
            if (source == null)
                throw new Exception("Invalid somehow");

            var expression = new ParametersToConstantVisitor<TSource>(source).Visit(_equivilentExpression) as LambdaExpression;
            return Expression.Lambda<Func<TDestination, bool>>(expression.Body, _equivilentExpression.Parameters[1]);
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