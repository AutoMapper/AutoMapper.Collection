using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.EquivilencyExpression;
using AutoMapper.Mappers;
using FluentAssertions;

namespace AutoMapper.Collection.Tests
{
    public class MapCollectionWithEquality
    {
        public MapCollectionWithEquality()
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<CollectionProfile>();
                x.CreateMap<ThingDto, Thing>().EqualityComparision((dto, entity) => dto.ID == entity.ID);
            });
        }

        public void Should_Update_Existing_Item()
        {
            var dtos = new List<ThingDto>()
            {
                new ThingDto() { ID = 1, Title = "test0" },
                new ThingDto() { ID = 2, Title = "test2" }
            };

            var items = new List<Thing>()
            {
                new Thing() { ID = 1, Title = "test1" },
                new Thing() { ID = 3, Title = "test3" },
            };

            Mapper.Map(dtos, items).Should().HaveElementAt(0, items.First());
        }
        public void Should_Work_With_Null_Destination()
        {
            var dtos = new List<ThingDto>()
            {
                new ThingDto() { ID = 1, Title = "test0" },
                new ThingDto() { ID = 2, Title = "test2" }
            };
            
            Mapper.Map<List<Thing>>(dtos).Should().HaveSameCount(dtos);
        }
    }
}
