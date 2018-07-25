using System.Collections.Generic;
using AutoMapper.EquivalencyExpression;
using FluentAssertions;
using Xunit;

namespace AutoMapper.Collection
{
    public class NullableIdTests
    {
        [Fact]
        public void Should_Work_With_Null_Id()
        {
            Mapper.Reset();
            Mapper.Initialize(x =>
            {
                x.AddCollectionMappers();
                x.CreateMap<ThingWithStringIdDto, ThingWithStringId>().EqualityComparison((dto, entity) => dto.ID == entity.ID);
            });

            var original = new List<ThingWithStringId>
            {
                new ThingWithStringId { ID = "1", Title = "test0" },
                new ThingWithStringId { ID = "2", Title = "test2" },
            };

            var dtos = new List<ThingWithStringIdDto>
            {
                new ThingWithStringIdDto { ID = "1", Title = "test0" },
                new ThingWithStringIdDto { ID = "2", Title = "test2" },
                new ThingWithStringIdDto { Title = "test3" }
            };

            Mapper.Map(dtos, original);

            original.Should().HaveSameCount(dtos);
        }

        public class ThingWithStringId
        {
            public string ID { get; set; }
            public string Title { get; set; }
            public override string ToString() { return Title; }
        }

        public class ThingWithStringIdDto
        {
            public string ID { get; set; }
            public string Title { get; set; }
        }
    }
}
