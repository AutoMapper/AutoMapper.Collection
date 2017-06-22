using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper.Collection.Internal.Extensions;

namespace AutoMapper.Collection.Internal.Mapper
{
    public class EqualityCollectionExpression<TSource, TDestination> : ICollectionExpression<TSource, TDestination>
    {
        public EqualityCollectionExpression(Expression<Func<TSource, TDestination, bool>> equivalentExpression)
        {
            var sourceParameter = equivalentExpression.Parameters[0];
            var destinationParameter = equivalentExpression.Parameters[1];

            var members = MemberExpressionExpando.Expand(sourceParameter, destinationParameter, equivalentExpression);

            SourceHashCodeExpression = members.Item1.GetHashCodeExpression<TSource>(sourceParameter);
            DestinationHashCodeExpression = members.Item2.GetHashCodeExpression<TDestination>(destinationParameter);
            EqualExpression = equivalentExpression;
        }

        public Expression<Func<TDestination, int>> DestinationHashCodeExpression { get; }
        public Expression<Func<TSource, TDestination, bool>> EqualExpression { get; }
        public Expression<Func<TSource, int>> SourceHashCodeExpression { get; }

        private class MemberExpressionExpando : ExpressionVisitor
        {
            private readonly List<Expression> _destinationMembers = new List<Expression>();
            private readonly ParameterExpression _destinationParameter;
            private readonly List<Expression> _sourceMembers = new List<Expression>();
            private readonly ParameterExpression _sourceParameter;

            private MemberExpressionExpando(ParameterExpression sourceParameter, ParameterExpression destinationParameter)
            {
                _sourceParameter = sourceParameter;
                _destinationParameter = destinationParameter;
            }

            public static Tuple<List<Expression>, List<Expression>> Expand(ParameterExpression sourceParameter, ParameterExpression destinationParameter, Expression expression)
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
}
