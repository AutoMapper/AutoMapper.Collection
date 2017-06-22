using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Collection;

namespace AutoMapper.EquivalencyExpression
{
    internal class EquivalentExpressionProperty<TSource, TDestination> : IEquivalentExpression<TSource, TDestination>
        where TSource : class
        where TDestination : class
    {
        private readonly Func<TDestination, int> _destinationPropertyGetHashCodeFunc;
        private readonly Expression<Func<TSource, TDestination, bool>> _equivalentExpression;
        private readonly Func<TSource, TDestination, bool> _propertyEqualFunc;
        private readonly Func<TSource, int> _sourcePropertyGetHashCodeFunc;

        public EquivalentExpressionProperty(Expression<Func<TSource, object>> sourceProperty, Expression<Func<TDestination, object>> destinationProperty)
        {
            var sourceMembers = MemberExpresssionExtractor.GetExpressions(sourceProperty);
            var destinationMembers = MemberExpresssionExtractor.GetExpressions(destinationProperty);

            var sourcePropertyGetHashCode = GetHashCodeExpression(sourceProperty, sourceMembers);
            _sourcePropertyGetHashCodeFunc = sourcePropertyGetHashCode.Compile();

            var destinationPropertyGetHashCode = GetHashCodeExpression(destinationProperty, destinationMembers);
            _destinationPropertyGetHashCodeFunc = destinationPropertyGetHashCode.Compile();

            _equivalentExpression = GetEqualExpression(sourceProperty, destinationProperty, sourceMembers, destinationMembers);
            _propertyEqualFunc = _equivalentExpression.Compile();
        }

        public bool IsEquivalent(TSource source, TDestination destination)
        {
            return _sourcePropertyGetHashCodeFunc(source) == _destinationPropertyGetHashCodeFunc(destination);
        }

        TDestinationItem IEquivalentExpression<TSource, TDestination>.Map<TSourceItem, TDestinationItem>(TSourceItem source, TDestinationItem destination, ResolutionContext context)
        {
            if (source == null || destination == null)
            {
                return destination;
            }

            var destList = destination.ToLookup(x => _destinationPropertyGetHashCodeFunc(x)).ToDictionary(x => x.Key, x => x.ToList());

            var items = source.Select(x =>
            {
                var sourceHash = _sourcePropertyGetHashCodeFunc(x);

                var item = default(TDestination);
                List<TDestination> itemList;
                if (destList.TryGetValue(sourceHash, out itemList))
                {
                    item = itemList.FirstOrDefault(dest => _propertyEqualFunc(x, dest));
                    if (item != null)
                    {
                        itemList.Remove(item);
                    }
                }
                return new { SourceItem = x, DestinationItem = item };
            });

            foreach (var keypair in items)
            {
                if (keypair.DestinationItem == null)
                {
                    destination.Add((TDestination)context.Mapper.Map(keypair.SourceItem, null, typeof(TSource), typeof(TDestination), context));
                }
                else
                {
                    context.Mapper.Map(keypair.SourceItem, keypair.DestinationItem, context);
                }
            }

            foreach (var removedItem in destList.SelectMany(x => x.Value))
            {
                destination.Remove(removedItem);
            }

            return destination;
        }

        public Expression<Func<TDestination, bool>> ToSingleSourceExpression(TSource source)
        {
            if (source == null)
            {
                throw new Exception("Invalid somehow");
            }

            var expression = (LambdaExpression)new ParametersToConstantVisitor<TSource>(source).Visit(_equivalentExpression);
            return Expression.Lambda<Func<TDestination, bool>>(expression.Body, _equivalentExpression.Parameters[1]);
        }

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

        private Expression<Func<T, int>> GetHashCodeExpression<T>(Expression<Func<T, object>> source, List<Expression> members)
        {
            var sourceParam = source.Parameters.First();
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

        private class MemberExpresssionExtractor : ExpressionVisitor
        {
            private readonly List<Expression> _members;

            private MemberExpresssionExtractor()
            {
                _members = new List<Expression>();
            }

            public static List<Expression> GetExpressions(Expression expression)
            {
                var visitoro = new MemberExpresssionExtractor();
                visitoro.Visit(expression);
                return visitoro._members;
            }
            
            protected override Expression VisitNew(NewExpression node)
            {
                foreach (var argument in node.Arguments)
                {
                    _members.Add(Expression.Convert(argument, typeof(object)));
                }
                return node;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                _members.Add(Expression.Convert(node, typeof(object)));
                return node;
            }
        }
    }
}