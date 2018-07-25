using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.EquivalencyExpression;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AutoMapper.EntityFrameworkCore
{
    public class GenerateEntityFrameworkCorePrimaryKeyPropertyMaps<TDatabaseContext> : IGeneratePropertyMaps
     where TDatabaseContext : DbContext, new()
    {
        private readonly TDatabaseContext _context = new TDatabaseContext();

        public IEnumerable<PropertyMap> GeneratePropertyMaps(TypeMap typeMap)
        {
            var propertyMaps = typeMap.GetPropertyMaps();

            var keyMembers = _context.Model.FindEntityType(typeMap.DestinationType)?.FindPrimaryKey().Properties ?? new List<IProperty>();
            return keyMembers.Select(m => Array.Find(propertyMaps, p => p.DestinationProperty.Name == m.Name));
        }
    }
}