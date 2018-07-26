using System;
using System.Linq;
using AutoMapper.EntityFrameworkCore;
using AutoMapper.EquivalencyExpression;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AutoMapper.Collection.EntityFrameworkCore.Tests
{
    public abstract class EntityFramworkCoreTestsBase
    {
        protected abstract DBContextBase GetDbContext();

        [Fact]
        public void Should_Persist_To_Update()
        {
            var db = GetDbContext();
            db.Things.Add(new Thing { Title = "Test2" });
            db.Things.Add(new Thing { Title = "Test3" });
            db.Things.Add(new Thing { Title = "Test4" });
            db.SaveChanges();

            Assert.Equal(3, db.Things.Count());

            var item = db.Things.First();

            db.Things.Persist().InsertOrUpdate(new ThingDto { ID = item.ID, Title = "Test" });
            Assert.Equal(1, db.ChangeTracker.Entries<Thing>().Count(x => x.State == EntityState.Modified));

            Assert.Equal(3, db.Things.Count());

            db.Things.First(x => x.ID == item.ID).Title.Should().Be("Test");
        }

        [Fact]
        public void Should_Persist_To_Insert()
        {
            var db = GetDbContext();
            db.Things.Add(new Thing { Title = "Test2" });
            db.Things.Add(new Thing { Title = "Test3" });
            db.Things.Add(new Thing { Title = "Test4" });
            db.SaveChanges();

            Assert.Equal(3, db.Things.Count());

            db.Things.Persist().InsertOrUpdate(new ThingDto { Title = "Test" });
            Assert.Equal(3, db.Things.Count());
            Assert.Equal(1, db.ChangeTracker.Entries<Thing>().Count(x => x.State == EntityState.Added));

            db.SaveChanges();

            Assert.Equal(4, db.Things.Count());

            db.Things.OrderByDescending(x => x.ID).First().Title.Should().Be("Test");
        }

        public abstract class DBContextBase : DbContext
        {
            protected DBContextBase(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected DBContextBase() { }

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
