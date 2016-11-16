using System;
using System.Linq.Expressions;

namespace AutoMapper.EquivilencyExpression
{
    public interface IEquivilentExpression
    {
        
    }

    public interface IEquivilentSoftDeleteExpression { }

    public interface IEquivilentExpression<TSource, TDestination> : IEquivilentExpression
    {
        bool IsEquivlent(TSource source, TDestination destination);
        Expression<Func<TDestination, bool>> ToSingleSourceExpression(TSource destination);
    }

    public interface IEquivilentSoftDeleteExpression<TSource, TDestination> : IEquivilentExpression<TSource, TDestination>, IEquivilentSoftDeleteExpression
    {
        void SetSoftDeleteValue(TDestination detination);
    }
}