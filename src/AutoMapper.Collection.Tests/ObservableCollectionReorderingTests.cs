using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.EquivalencyExpression;
using FluentAssertions;
using Xunit;

namespace AutoMapper.Collection
{
    public class ObservableCollectionReorderingTests : MappingTestBase
    {
        private static void Configure(IMapperConfigurationExpression cfg)
        {
            cfg.AddCollectionMappers();
            cfg.CreateMap<MapCollectionWithEqualityTests.ThingDto, MapCollectionWithEqualityTests.Thing>()
               .EqualityComparison((MapCollectionWithEqualityTests.ThingDto dto, MapCollectionWithEqualityTests.Thing entity) => dto.ID == entity.ID);
        }

        [Fact]
        public void Reordering_should_raise_Move_event_for_existing_items()
        {
            var mapper = CreateMapper(Configure);

            // Source in desired order
            var source = new[]
            {
                new MapCollectionWithEqualityTests.ThingDto { ID = 1 },
                new MapCollectionWithEqualityTests.ThingDto { ID = 2 }
            };

            // Destination has the same two existing instances but in reverse order
            var a = new MapCollectionWithEqualityTests.Thing { ID = 1 };
            var b = new MapCollectionWithEqualityTests.Thing { ID = 2 };
            var destination = new ObservableCollection<MapCollectionWithEqualityTests.Thing> { b, a };

            var events = new List<NotifyCollectionChangedEventArgs>();
            destination.CollectionChanged += (_, e) => events.Add(e);

            // Act
            var result = mapper.Map(source, destination);

            // Basic correctness checks
            result.Should().BeSameAs(destination);
            result.Select(t => t.ID).Should().ContainInOrder(1, 2);
            result[0].Should().BeSameAs(a);
            result[1].Should().BeSameAs(b);

            // Event expectations: a single Move from index 1 to 0 for item 'a'
            events.Should().HaveCount(1);
            events[0].Action.Should().Be(NotifyCollectionChangedAction.Move);
            events[0].OldStartingIndex.Should().Be(1);
            events[0].NewStartingIndex.Should().Be(0);
            events[0].OldItems![0].Should().BeSameAs(a);
        }

        [Fact]
        public void No_reordering_should_not_raise_any_events_for_equal_order()
        {
            var mapper = CreateMapper(Configure);

            var source = new[]
            {
                new MapCollectionWithEqualityTests.ThingDto { ID = 1 },
                new MapCollectionWithEqualityTests.ThingDto { ID = 2 }
            };

            var a = new MapCollectionWithEqualityTests.Thing { ID = 1 };
            var b = new MapCollectionWithEqualityTests.Thing { ID = 2 };
            var destination = new ObservableCollection<MapCollectionWithEqualityTests.Thing> { a, b };

            var events = new List<NotifyCollectionChangedEventArgs>();
            destination.CollectionChanged += (_, e) => events.Add(e);

            var result = mapper.Map(source, destination);

            result.Should().BeSameAs(destination);
            result.Select(t => t.ID).Should().ContainInOrder(1, 2);
            events.Should().BeEmpty();
        }

        [Fact]
        public void Complex_reordering_should_raise_only_Move_events()
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
            var destination = new ObservableCollection<MapCollectionWithEqualityTests.Thing> { c, a, b };

            var events = new List<NotifyCollectionChangedEventArgs>();
            destination.CollectionChanged += (_, e) => events.Add(e);

            var result = mapper.Map(source, destination);

            result.Should().BeSameAs(destination);
            result.Select(t => t.ID).Should().ContainInOrder(1, 2, 3);
            result[0].Should().BeSameAs(a);
            result[1].Should().BeSameAs(b);
            result[2].Should().BeSameAs(c);

            // Expect two Move events (reorder existing items), no Add/Remove
            events.Should().HaveCount(2);
            events.Should().OnlyContain(e => e.Action == NotifyCollectionChangedAction.Move);
        }

        [Fact]
        public void Reordering_with_new_items_should_raise_Add_and_Move_events()
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
            var destination = new ObservableCollection<MapCollectionWithEqualityTests.Thing> { b, a };

            var events = new List<NotifyCollectionChangedEventArgs>();
            destination.CollectionChanged += (_, e) => events.Add(e);

            var result = mapper.Map(source, destination);

            result.Select(t => t.ID).Should().ContainInOrder(1, 4, 2);
            result[0].Should().BeSameAs(a);
            result[2].Should().BeSameAs(b);

            // Expect: Move(a 1->0), Add(4 at 1), Move(b 1->2)
            events.Count(e => e.Action == NotifyCollectionChangedAction.Move).Should().Be(2);
            events.Count(e => e.Action == NotifyCollectionChangedAction.Add).Should().Be(1);
            events.Count(e => e.Action == NotifyCollectionChangedAction.Remove).Should().Be(0);
        }

        [Fact]
        public void Removing_unmatched_items_then_no_reorder_should_raise_only_Remove()
        {
            var mapper = CreateMapper(Configure);

            var source = new[]
            {
                new MapCollectionWithEqualityTests.ThingDto { ID = 1 },
                new MapCollectionWithEqualityTests.ThingDto { ID = 2 }
            };

            var a = new MapCollectionWithEqualityTests.Thing { ID = 1 };
            var b = new MapCollectionWithEqualityTests.Thing { ID = 2 };
            var extra = new MapCollectionWithEqualityTests.Thing { ID = 3 };
            var destination = new ObservableCollection<MapCollectionWithEqualityTests.Thing> { extra, a, b };

            var events = new List<NotifyCollectionChangedEventArgs>();
            destination.CollectionChanged += (_, e) => events.Add(e);

            var result = mapper.Map(source, destination);

            result.Select(t => t.ID).Should().ContainInOrder(1, 2);
            events.Count(e => e.Action == NotifyCollectionChangedAction.Remove).Should().Be(1);
            events.Count(e => e.Action == NotifyCollectionChangedAction.Move).Should().Be(0);
            events.Count(e => e.Action == NotifyCollectionChangedAction.Add).Should().Be(0);
        }
    }
}
