using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlServerCe;
using System.Linq;
using AutoMapper.EntityFramework;
using AutoMapper.EquivalencyExpression;
using FluentAssertions;
using Xunit;

namespace AutoMapper.Collection.EntityFramework.Tests
{
    public class EntityFramworkTests : MappingTestBase
    {
        private void ConfigureMapper(IMapperConfigurationExpression cfg)
        {
            cfg.AddCollectionMappers();
            cfg.CreateMap<ThingDto, Thing>().ReverseMap();
            cfg.SetGeneratePropertyMaps<GenerateEntityFrameworkPrimaryKeyPropertyMaps<DB>>();
        }

        [Fact]
        public void Should_Persist_To_Update()
        {
            var mapper = CreateMapper(ConfigureMapper);

            var db = new DB();
            db.Things.Add(new Thing { Title = "Test2" });
            db.Things.Add(new Thing { Title = "Test3" });
            db.Things.Add(new Thing { Title = "Test4" });
            db.SaveChanges();

            Assert.Equal(3, db.Things.Count());

            var item = db.Things.First();

            db.Things.Persist(mapper).InsertOrUpdate(new ThingDto { ID = item.ID, Title = "Test" });
            Assert.Equal(1, db.ChangeTracker.Entries<Thing>().Count(x => x.State == EntityState.Modified));

            Assert.Equal(3, db.Things.Count());

            db.Things.First(x => x.ID == item.ID).Title.Should().Be("Test");
        }

        [Fact]
        public void Should_Persist_To_Insert()
        {
            var mapper = CreateMapper(ConfigureMapper);

            var db = new DB();
            db.Things.Add(new Thing { Title = "Test2" });
            db.Things.Add(new Thing { Title = "Test3" });
            db.Things.Add(new Thing { Title = "Test4" });
            db.SaveChanges();

            Assert.Equal(3, db.Things.Count());

            db.Things.Persist(mapper).InsertOrUpdate(new ThingDto { Title = "Test" });
            Assert.Equal(3, db.Things.Count());
            Assert.Equal(1, db.ChangeTracker.Entries<Thing>().Count(x => x.State == EntityState.Added));

            db.SaveChanges();

            Assert.Equal(4, db.Things.Count());

            db.Things.OrderByDescending(x => x.ID).First().Title.Should().Be("Test");
        }

        public class DB : DbContext
        {
            public DB()
                : base(new SqlCeConnection("Data Source=MyDatabase.sdf;Persist Security Info=False;"), contextOwnsConnection: true)
            {
                Things.RemoveRange(Things);
                SaveChanges();
            }

            public DbSet<Thing> Things { get; set; }
        }

        public class Thing
        {
            public int ID { get; set; }
            public string Title { get; set; }
            public override string ToString() { return Title; }
        }

        public class ThingDto
        {
            public int ID { get; set; }
            public string Title { get; set; }
        }
    }
}
