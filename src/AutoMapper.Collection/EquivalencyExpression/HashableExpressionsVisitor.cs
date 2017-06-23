using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoMapper.EquivalencyExpression
{
    internal class HashableExpressionsVisitor : ExpressionVisitor
    {
        private readonly List<Expression> _destinationMembers = new List<Expression>();
        private readonly ParameterExpression _destinationParameter;
        private readonly List<Expression> _sourceMembers = new List<Expression>();
        private readonly ParameterExpression _sourceParameter;

        internal HashableExpressionsVisitor(ParameterExpression sourceParameter, ParameterExpression destinationParameter)
        {
            _sourceParameter = sourceParameter;
            _destinationParameter = destinationParameter;
        }

        internal static Tuple<List<Expression>, List<Expression>> Expand(ParameterExpression sourceParameter, ParameterExpression destinationParameter, Expression expression)
        {
            var visitor = new HashableExpressionsVisitor(sourceParameter, destinationParameter);
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
            return node;
        }
    }
}