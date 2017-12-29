using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AutoMapper.EntityFramework;
using AutoMapper.EquivalencyExpression;
using FluentAssertions;
using Xunit;

namespace AutoMapper.Collection.EntityFramework.Tests
{
    public class MapCollectionWithEqualityTests
    {
        public MapCollectionWithEqualityTests()
        {
            Mapper.Reset();
            Mapper.Initialize(x =>
            {
                x.AddCollectionMappers();
                x.CreateMap<ThingDto, Thing>().ReverseMap();
                x.SetGeneratePropertyMaps<GenerateEntityFrameworkPrimaryKeyPropertyMaps<DB>>();
            });
        }

        [Fact]
        public void Should_Keep_Existing_List()
        {
            var dtos = new List<ThingDto>
            {
                new ThingDto { ID = 1, Title = "test0" },
                new ThingDto { ID = 2, Title = "test2" }
            };

            var items = new List<Thing>
            {
                new Thing { ID = 1, Title = "test1" },
                new Thing { ID = 3, Title = "test3" },
            };

            Mapper.Map(dtos, items).Should().BeSameAs(items);
        }

        [Fact]
        public void Should_Update_Existing_Item()
        {
            var dtos = new List<ThingDto>
            {
                new ThingDto { ID = 1, Title = "test0" },
                new ThingDto { ID = 2, Title = "test2" }
            };

            var items = new List<Thing>
            {
                new Thing { ID = 1, Title = "test1" },
                new Thing { ID = 3, Title = "test3" },
            };

            var cache = items.ToList();
            Mapper.Map(dtos, items.ToList()).Should().HaveElementAt(0, cache.First());
        }

        [Fact]
        public void Should_Work_With_Null_Destination()
        {
            var dtos = new List<ThingDto>
            {
                new ThingDto { ID = 1, Title = "test0" },
                new ThingDto { ID = 2, Title = "test2" }
            };

            Mapper.Map<List<Thing>>(dtos).Should().HaveSameCount(dtos);
        }

        [Fact]
        public void Should_Be_Instanced_Based()
        {
            Mapper.Reset();
            Mapper.Initialize(x =>
            {
                x.AddCollectionMappers();
                x.CreateMap<ThingDto, Thing>().ReverseMap();
            });

            var dtos = new List<ThingDto>
            {
                new ThingDto { ID = 1, Title = "test0" },
                new ThingDto { ID = 2, Title = "test2" }
            };

            var items = new List<Thing>
            {
                new Thing { ID = 1, Title = "test1" },
                new Thing { ID = 3, Title = "test3" },
            };

            var cache = items.ToList();
            Mapper.Map(dtos, items.ToList()).Should().NotContain(cache.First());
        }

        //[Fact]
        //public void Should_Persist_To_Update()
        //{
        //    var db = new DB();
        //    db.Things.Persist().InsertOrUpdate(new ThingDto { Title = "Test" });
        //    db.Things.First().Title.Should().Be("Test");
        //}

        public class DB : DbContext
        {
            static DB()
            {
                Database.SetInitializer<DB>(null);
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
