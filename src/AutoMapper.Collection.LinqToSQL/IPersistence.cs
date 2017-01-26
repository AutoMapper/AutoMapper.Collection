using System;

namespace AutoMapper.Collection.LinqToSQL
{
    public interface IPersistence
    {
        /// <summary>
        /// Insert Or Update the <see cref="T:System.Data.Entity.DbSet`1"/> with <paramref name="from"/>
        /// </summary>
        /// <remarks>Uses <see cref="AutoMapper.EquivalencyExpression.EquivalentExpressions.GenerateEquality"/>> to find equality between Source and From Types to determine if insert or update</remarks>
        /// <typeparam name="TFrom">Source Type mapping from</typeparam>
        /// <param name="from">Object to update to <see cref="T:System.Data.Entity.DbSet`1"/></param>
        void InsertOrUpdate<TFrom>(TFrom from) where TFrom : class;

        void InsertOrUpdate(Type type, object from);

        /// <summary>
        /// Remove from <see cref="T:System.Data.Entity.DbSet`1"/> with <paramref name="from"/>
        /// </summary>
        /// <remarks>Uses <see cref="AutoMapper.EquivalencyExpression.EquivalentExpressions.GenerateEquality"/>> to find equality between Source and From Types to determine if insert or update</remarks>
        /// <typeparam name="TFrom">Source Type mapping from</typeparam>
        /// <param name="from">Object to remove that is Equivalent in <see cref="T:System.Data.Entity.DbSet`1"/></param>
        void Remove<TFrom>(TFrom from) where TFrom : class;
    }
}