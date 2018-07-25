using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.EquivalencyExpression;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace AutoMapper.EntityFrameworkCore
{
    public class GenerateEntityFrameworkCorePrimaryKeyPropertyMapsDependencyInjection<TDatabaseContext>
        : IGeneratePropertyMaps
        where TDatabaseContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;

        public GenerateEntityFrameworkCorePrimaryKeyPropertyMapsDependencyInjection(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<PropertyMap> GeneratePropertyMaps(TypeMap typeMap)
        {
            using (var scope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var propertyMaps = typeMap.GetPropertyMaps();

                var context = ActivatorUtilities.GetServiceOrCreateInstance<TDatabaseContext>(scope.ServiceProvider);

                var keyMembers = context.Model.FindEntityType(typeMap.DestinationType)?.FindPrimaryKey().Properties ?? new List<IProperty>();
                return keyMembers.Select(m => Array.Find(propertyMaps, p => p.DestinationProperty.Name == m.Name));
            }
        }
    }
}