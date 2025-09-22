using System.Collections.Generic;
using System.Linq;
using AutoMapper.EquivalencyExpression;
using FluentAssertions;
using Xunit;

namespace AutoMapper.Collection
{
    public class NonListCollectionRebuildTests : MappingTestBase
    {
        private static void Configure(IMapperConfigurationExpression cfg)
        {
            cfg.AddCollectionMappers();
            cfg.CreateMap<MapCollectionWithEqualityTests.ThingDto, MapCollectionWithEqualityTests.Thing>()
               .EqualityComparison((MapCollectionWithEqualityTests.ThingDto dto, MapCollectionWithEqualityTests.Thing entity) => dto.ID == entity.ID);
        }

        [Fact]
        public void Should_rebuild_non_list_collection_in_source_order_preserving_instances()
        {
            var mapper = CreateMapper(Configure);

            var source = new[]
            {
                new MapCollectionWithEqualityTests.ThingDto { ID = 1 },
                new MapCollectionWithEqualityTests.ThingDto { ID = 2 },
                new MapCollectionWithEqualityTests.ThingDto { ID = 3 }
            };

            var a = new MapCollectionWithEqualityTests.Thing { ID = 1 };
            var b = new MapCollectionWithEqualityTests.Thing { ID = 2 };
            var c = new MapCollectionWithEqualityTests.Thing { ID = 3 };

            // Use LinkedList<T> which is ICollection<T> but not IList<T>, to trigger the fallback branch (lines 121-134)
            var destination = new LinkedList<MapCollectionWithEqualityTests.Thing>(new[] { b, a, c });

            var result = mapper.Map(source, destination);

            result.Should().BeSameAs(destination);

            // Verify order and that existing instances were preserved
            result.Select(t => t.ID).Should().ContainInOrder(1, 2, 3);
            result.ElementAt(0).Should().BeSameAs(a);
            result.ElementAt(1).Should().BeSameAs(b);
            result.ElementAt(2).Should().BeSameAs(c);
        }

        [Fact]
        public void Should_remove_extraneous_and_add_new_items_then_rebuild_in_order_for_non_list()
        {
            var mapper = CreateMapper(Configure);

            var source = new[]
            {
                new MapCollectionWithEqualityTests.ThingDto { ID = 1 },
                new MapCollectionWithEqualityTests.ThingDto { ID = 4 },
                new MapCollectionWithEqualityTests.ThingDto { ID = 2 }
            };

            var a = new MapCollectionWithEqualityTests.Thing { ID = 1 };
            var b = new MapCollectionWithEqualityTests.Thing { ID = 2 };
            var extra = new MapCollectionWithEqualityTests.Thing { ID = 3 };

            var destination = new LinkedList<MapCollectionWithEqualityTests.Thing>(new[] { b, a, extra });

            var result = mapper.Map(source, destination);

            // New collection should be in source order and preserve existing instances where matched
            result.Select(t => t.ID).Should().ContainInOrder(1, 4, 2);
            result.ElementAt(0).Should().BeSameAs(a);
            result.ElementAt(2).Should().BeSameAs(b);

            // Ensure the extraneous item was removed
            result.Should().NotContain(extra);

            // Ensure the new item (ID=4) was created and added (reference not equal to any existing)
            result.Count(t => t.ID == 4).Should().Be(1);
        }
    }
}
