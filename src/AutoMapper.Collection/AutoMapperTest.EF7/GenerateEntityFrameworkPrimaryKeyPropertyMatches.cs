using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper.EquivilencyExpression;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;

namespace AutoMapper.EntityFramework
{
    public class GenerateEntityFrameworkPrimaryKeyPropertyMaps<TDatabaseContext> : IGeneratePropertyMaps
        where TDatabaseContext : DbContext, new()
    {
        private readonly TDatabaseContext _context = new TDatabaseContext();

        public IEnumerable<PropertyMap> GeneratePropertyMaps(Type srcType, Type destType)
        {
            var mapper = Mapper.FindTypeMapFor(srcType, destType);
            var propertyMaps = mapper.GetPropertyMaps();

            var keyMembers = _context.Model.EntityTypes.FirstOrDefault(et => et.ClrType == destType)?.FindPrimaryKey().Properties ?? new List<IProperty>();
            var primaryKeyPropertyMatches = keyMembers.Select(m => propertyMaps.FirstOrDefault(p => p.DestinationProperty.Name == m.Name));

            return primaryKeyPropertyMatches;
        }
    }
}