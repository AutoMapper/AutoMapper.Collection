using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.EntityFramework;
using AutoMapper.EquivilencyExpression;
using AutoMapper.Mappers;
using FluentAssertions;

namespace AutoMapper.Collection.EntityFramework.Tests
{
    public class MapCollectionWithEqualityTests
    {
        public MapCollectionWithEqualityTests()
        {
            Mapper.Initialize(x =>
            {
                //x.AddProfile<CollectionProfile>();
                x.CreateMap<ThingDto, Thing>();//.EqualityComparision((dto, entity) => dto.ID == entity.ID);
            });
            //EquivilentExpressions.GenerateEquality.Add(new GenerateEntityFrameworkPrimaryKeyEquivilentExpressions<DB>());
        }

        public class DB : DbContext
        {
            public DbSet<ThingDto> Things { get; set; }
        }

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

            Mapper.Map(dtos, items).Should().HaveElementAt(0, items.First());
        }

        public void Should_Work_With_Null_Destination()
        {
            var dtos = new List<ThingDto>
            {
                new ThingDto { ID = 1, Title = "test0" },
                new ThingDto { ID = 2, Title = "test2" }
            };

            Mapper.Map<List<Thing>>(dtos).Should().HaveSameCount(dtos);
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
