using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoMapper.EquivilencyExpression
{
    internal class UserDefinedEquivilentExpressions : IGenerateEquivilentExpressions
    {
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, IEquivilentExpression>> _equivilentExpressionDictionary = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, IEquivilentExpression>>();

        public bool CanGenerateEquivilentExpression(Type sourceType, Type destinationType)
        {
            return GetEquivlelentExpression(sourceType, destinationType) != null;
        }

        public IEquivilentExpression GeneratEquivilentExpression(Type sourceType, Type destinationType)
        {
            return GetEquivlelentExpression(sourceType, destinationType);
        }

        private IEquivilentExpression GetEquivlelentExpression(Type srcType, Type destType)
        {
            ConcurrentDictionary<Type, IEquivilentExpression> destMap;
            IEquivilentExpression srcExpression;
            if (_equivilentExpressionDictionary.TryGetValue(destType, out destMap) && destMap.TryGetValue(srcType, out srcExpression))
            {
                return srcExpression;
            }
            return null;
        }

        internal void AddEquivilencyExpression<TSource, TDestination>(Expression<Func<TSource, TDestination, bool>> equivilentExpression)
            where TSource : class
            where TDestination : class
        {
            var destinationDictionary = _equivilentExpressionDictionary.GetOrAdd(typeof(TDestination), t => new ConcurrentDictionary<Type, IEquivilentExpression>());
            destinationDictionary.AddOrUpdate(typeof(TSource), new EquivilentExpression<TSource, TDestination>(equivilentExpression), (type, old) => new EquivilentExpression<TSource, TDestination>(equivilentExpression));
        }
    }
}