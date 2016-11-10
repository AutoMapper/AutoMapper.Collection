using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.EquivilencyExpression;
using AutoMapper.Mappers;
using FluentAssertions;

namespace AutoMapper.Collection
{
    public class MapCollectionWithEqualityTests
    {
        public MapCollectionWithEqualityTests()
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<CollectionProfile>();
                x.CreateMap<ThingDto, Thing>().EqualityComparision((dto, entity) => dto.ID == entity.ID);
                x.CreateMap<AnotherThingDto, AnotherThing>().EqualityComparision((dto, entity) => dto.ID == entity.ID, (entity,isdeleted) => entity.IsDeleted = true);
            });
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

        public void Should_Update_IsDeleted_From_Removed_Item()
        {
            var dtos = new List<AnotherThingDto>
            {
                new AnotherThingDto { ID = 1, Title = "test0" },
                new AnotherThingDto { ID = 2, Title = "test2" }
            };

            var items = new List<AnotherThing>
            {
                new AnotherThing { ID = 1, Title = "test1", IsDeleted = false },
                new AnotherThing { ID = 3, Title = "test3", IsDeleted = false },
            };

            Mapper.Map(dtos, items).Should().HaveElementAt(0, items.First()).And.HaveCount(3);
            items[0].IsDeleted.Should().BeFalse();
            items[1].IsDeleted.Should().BeTrue();
            items[2].IsDeleted.Should().BeFalse();
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

        public class AnotherThing
        {
            public int ID { get; set; }
            public string Title { get; set; }
            public bool IsDeleted { get; set; }
            public override string ToString() { return Title; }
        }

        public class AnotherThingDto
        {
            public int ID { get; set; }
            public string Title { get; set; }
        }
    }
}
