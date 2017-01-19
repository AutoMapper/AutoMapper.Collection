using AutoMapper.EquivilencyExpression;
using AutoMapper.Mappers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace AutoMapper.Collection
{
    [TestClass]
    public class OptionsTest
    {
        [TestMethod]
        public void Should_Retain_Options_Passed_In_Map()
        {
            var collectionTestValue = 0;
            var collectionMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddCollectionMappers();
                cfg.CreateMap<ThingDto, Thing>().EqualityComparision((dto, entity) => dto.ID == entity.ID).AfterMap((_, __, ctx) => collectionTestValue = (int)ctx.Options.Items["Test"]);
            }).CreateMapper();

            var normalTestValue = 0;
            var normalMapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ThingDto, Thing>().AfterMap((_, __, ctx) => normalTestValue = (int)ctx.Options.Items["Test"]);
            }).CreateMapper();

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

            collectionMapper.Map(dtos, items, opts => opts.Items.Add("Test", 1));
            normalMapper.Map(dtos, items, opts => opts.Items.Add("Test", 1));

            collectionTestValue.ShouldBeEquivalentTo(1);
            normalTestValue.ShouldBeEquivalentTo(1);
        }

        public class Thing
        {
            public int ID { get; set; }
            public string Title { get; set; }

            public override string ToString()
            {
                return Title;
            }
        }

        public class ThingDto
        {
            public int ID { get; set; }
            public string Title { get; set; }
        }
    }
}