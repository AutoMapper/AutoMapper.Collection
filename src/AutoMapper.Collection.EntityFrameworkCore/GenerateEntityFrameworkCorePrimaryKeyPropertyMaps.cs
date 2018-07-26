using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.EquivalencyExpression;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace AutoMapper.EntityFrameworkCore
{
    public class GenerateEntityFrameworkCorePrimaryKeyPropertyMaps<TDatabaseContext>
        : IGeneratePropertyMaps
        where TDatabaseContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;

        public GenerateEntityFrameworkCorePrimaryKeyPropertyMaps(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public GenerateEntityFrameworkCorePrimaryKeyPropertyMaps() { }

        public IEnumerable<PropertyMap> GeneratePropertyMaps(TypeMap typeMap)
        {
            using (var holder = new DbContextHolder(_serviceProvider))
            {
                var propertyMaps = typeMap.GetPropertyMaps();

                var keyMembers = holder.DbContext.Model.FindEntityType(typeMap.DestinationType)?.FindPrimaryKey().Properties ?? new List<IProperty>();
                return keyMembers.Select(m => Array.Find(propertyMaps, p => p.DestinationProperty.Name == m.Name));
            }
        }

        private class DbContextHolder : IDisposable
        {
            private readonly IDisposable _disposable;

            public DbContextHolder(IServiceProvider serviceProvider)
            {
                if (serviceProvider == null)
                {
                    DbContext = (DbContext)Activator.CreateInstance(typeof(TDatabaseContext));
                    _disposable = DbContext;
                }
                else
                {
                    var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
                    _disposable = scope;
                    DbContext = ActivatorUtilities.GetServiceOrCreateInstance<TDatabaseContext>(scope.ServiceProvider);
                }
            }

            public DbContext DbContext { get; }

            public void Dispose() => _disposable.Dispose();
        }
    }
}