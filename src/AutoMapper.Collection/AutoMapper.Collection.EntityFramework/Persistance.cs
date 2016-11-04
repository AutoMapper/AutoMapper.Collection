using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.EntityFramework
{
    public class Persistance<TTo> : IPersistance
        where TTo : class
    {
        private readonly DbSet<TTo> _sourceSet;
        private readonly IMapper _mapper;

        public Persistance(DbSet<TTo> sourceSet, IMapper mapper)
        {
            _sourceSet = sourceSet;
            _mapper = mapper;
        }

        public void InsertOrUpdate<TFrom>(TFrom from)
            where TFrom : class
        {
            InsertOrUpdate(typeof(TFrom), from);
        }
        public void InsertOrUpdate(Type type, object from)
        {
            var equivExpr = _mapper == null
                ? Mapper.Map(from, type, typeof(Expression<Func<TTo, bool>>)) as Expression<Func<TTo, bool>>
                : _mapper.Map(from, type, typeof(Expression<Func<TTo, bool>>)) as Expression<Func<TTo, bool>>;
            if (equivExpr == null)
                return;

            var to = _sourceSet.FirstOrDefault(equivExpr);

            if (to == null)
            {
                to = _sourceSet.Create<TTo>();
                _sourceSet.Add(to);
            }
            if (_mapper == null)
                Mapper.Map(from, to);
            else
                _mapper.Map(from,to);
        }

        public void Remove<TFrom>(TFrom from)
            where TFrom : class
        {
            var equivExpr = _mapper == null
                ? Mapper.Map<TFrom, Expression<Func<TTo, bool>>>(from)
                : _mapper.Map<TFrom, Expression<Func<TTo, bool>>>(from);
            if (equivExpr == null)
                return;
            var to = _sourceSet.FirstOrDefault(equivExpr);

            if (to != null)
                _sourceSet.Remove(to);
        }
    }
}