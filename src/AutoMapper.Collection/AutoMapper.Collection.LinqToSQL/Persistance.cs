using System;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.Collection.LinqToSQL
{
    public class Persistance<TTo> : IPersistance
        where TTo : class
    {
        private readonly Table<TTo> _sourceSet;
        private readonly IMappingEngine _mappingEngine;

        public Persistance(Table<TTo> sourceSet, IMappingEngine mappingEngine)
        {
            _mappingEngine = mappingEngine;
            _sourceSet = sourceSet;
        }

        public void InsertOrUpdate<TFrom>(TFrom from)
            where TFrom : class
        {
            InsertOrUpdate(typeof(TFrom), from);
        }

        public void InsertOrUpdate(Type type, object from)
        {
            var equivExpr = Mapper.Map(from, type, typeof(Expression<Func<TTo, bool>>)) as Expression<Func<TTo, bool>>;
            if (equivExpr == null)
                return;

            var to = _sourceSet.FirstOrDefault(equivExpr);

            if (to == null)
            {
                to = Activator.CreateInstance<TTo>();
                _sourceSet.InsertOnSubmit(to);
            }
            _mappingEngine.Map(from, to);
        }

        public void Remove<TFrom>(TFrom from)
            where TFrom : class
        {
            var equivExpr = Mapper.Map<TFrom, Expression<Func<TTo, bool>>>(from);
            if (equivExpr == null)
                return;
            var to = _sourceSet.FirstOrDefault(equivExpr);

            if (to != null)
                _sourceSet.DeleteOnSubmit(to);
        }
    }
}