using System;
using AutoMapper.EntityFrameworkCore;
using AutoMapper.EquivalencyExpression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AutoMapper.Collection.EntityFrameworkCore.Tests
{
    public class EntityFramworkCoreUsingDITests : EntityFramworkCoreTestsBase, IDisposable
    {
        private readonly IServiceScope _serviceScope;

        public EntityFramworkCoreUsingDITests()
        {
            var services = new ServiceCollection();
            services.AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<DB>(options => options.UseInMemoryDatabase("EfTestDatabase" + Guid.NewGuid()));
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            Mapper.Reset();
            Mapper.Initialize(x =>
            {
                x.ConstructServicesUsing(type => ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, type));
                x.AddCollectionMappers();
                x.SetGeneratePropertyMaps<GenerateEntityFrameworkCorePrimaryKeyPropertyMaps<DB>>();
            });

            _serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        }

        public void Dispose()
        {
            _serviceScope?.Dispose();
        }

        protected override DBContextBase GetDbContext()
        {
            return _serviceScope.ServiceProvider.GetRequiredService<DB>();
        }

        public class DB : DBContextBase
        {
            public DB(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }
        }
    }
}
