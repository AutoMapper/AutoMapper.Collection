using System.Data.Entity;

namespace AutoMapper.EntityFramework
{
    public static class Extensions
    {
        /// <summary>
        /// Create a persistance object for the <see cref="T:System.Data.Entity.DbSet`1"/> to have data persisted or removed from
        /// </summary>
        /// <typeparam name="TSource">Source table type to be updated</typeparam>
        /// <param name="source">DbSet to be updated</param>
        /// <returns>Persistance object to Update or Remove data</returns>
        public static IPersistance Persist<TSource>(this DbSet<TSource> source)
            where TSource : class
        {
            return new Persistance<TSource>(source, Mapper.Engine);
        }
    }
}