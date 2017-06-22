using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Collection.Internal.Extensions;

namespace AutoMapper.Collection.Internal.Mapper
{
    public class ObjectCollectionExpression<TSource, TDestination> : ICollectionExpression<TSource, TDestination>
    {
        public ObjectCollectionExpression(Expression<Func<TSource, object>> sourceProperty, Expression<Func<TDestination, object>> destinationProperty)
        {
            var sourceParameter = sourceProperty.Parameters.First();
            var destinationParameter = destinationProperty.Parameters.First();

            var sourceMembers = MemberExpresssionExtractor.GetExpressions(sourceProperty);
            var destinationMembers = MemberExpresssionExtractor.GetExpressions(destinationProperty);

            SourceHashCodeExpression = sourceMembers.GetHashCodeExpression<TSource>(sourceParameter);
            DestinationHashCodeExpression = destinationMembers.GetHashCodeExpression<TDestination>(destinationParameter);
            EqualExpression = GetEqualExpression(sourceProperty, destinationProperty, sourceMembers, destinationMembers);
        }

        public Expression<Func<TDestination, int>> DestinationHashCodeExpression { get; }
        public Expression<Func<TSource, TDestination, bool>> EqualExpression { get; }
        public Expression<Func<TSource, int>> SourceHashCodeExpression { get; }

        private Expression<Func<TSource, TDestination, bool>> GetEqualExpression(Expression<Func<TSource, object>> source, Expression<Func<TDestination, object>> destination, List<Expression> sourceMembers, List<Expression> destinationMembers)
        {
            var sourceParam = source.Parameters.First();
            var destinationParam = destination.Parameters.First();

            var returnVariable = Expression.Variable(typeof(bool), "result");
            var returnTarget = Expression.Label(typeof(bool));
            var returnExpression = Expression.Return(returnTarget, returnVariable, typeof(bool));
            var returnLabel = Expression.Label(returnTarget, Expression.Constant(false));

            var ewualsMethod = typeof(object).GetDeclaredMethod(nameof(Equals));

            var expressions = new List<Expression>();
            var sourceCount = sourceMembers.Count;
            var destinationCount = destinationMembers.Count;

            if (sourceCount != destinationCount)
            {
                throw new ArgumentException("Source and destination have different property count.");
            }

            for (var i = 0; i < sourceCount; i++)
            {
                var sourceMember = sourceMembers[i];
                var destinationMember = destinationMembers[i];
                if (sourceMember.Type != destinationMember.Type)
                {
                    throw new ArgumentException("Source and destination member have different type.");
                }

                var sourceEqualDestination = Expression.Call(sourceMember, ewualsMethod, destinationMember);
                if (expressions.Count == 0)
                {
                    expressions.Add(Expression.Assign(returnVariable, sourceEqualDestination));
                }
                else
                {
                    expressions.Add(Expression.AndAssign(returnVariable, sourceEqualDestination));
                }
            }

            expressions.Add(returnExpression);
            expressions.Add(returnLabel);

            var resutltBlock = Expression.Block(new[] { returnVariable }, expressions);
            return Expression.Lambda<Func<TSource, TDestination, bool>>(resutltBlock, sourceParam, destinationParam);
        }

        private class MemberExpresssionExtractor : ExpressionVisitor
        {
            private readonly List<Expression> _members = new List<Expression>();

            private MemberExpresssionExtractor()
            {
            }

            public static List<Expression> GetExpressions(Expression expression)
            {
                var visitor = new MemberExpresssionExtractor();
                visitor.Visit(expression);
                return visitor._members;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                _members.Add(Expression.Convert(node, typeof(object)));
                return node;
            }

            protected override Expression VisitNew(NewExpression node)
            {
                foreach (var argument in node.Arguments)
                {
                    _members.Add(Expression.Convert(argument, typeof(object)));
                }

                return node;
            }
        }
    }
}
