using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.EquivalencyExpression;
using FluentAssertions;
using Xunit;

namespace AutoMapper.Collection
{
    public class IListInsertionTests : MappingTestBase
    {
        private static void Configure(IMapperConfigurationExpression cfg)
        {
            cfg.AddCollectionMappers();
            cfg.CreateMap<MapCollectionWithEqualityTests.ThingDto, MapCollectionWithEqualityTests.Thing>()
               .EqualityComparison((MapCollectionWithEqualityTests.ThingDto dto, MapCollectionWithEqualityTests.Thing entity) => dto.ID == entity.ID);
        }

        [Fact]
        public void Should_insert_new_items_at_correct_position_in_IList()
        {
            var mapper = CreateMapper(Configure);

            // Source requires inserting a new item (ID=4) between two existing ones (1 and 2)
            var source = new[]
            {
                new MapCollectionWithEqualityTests.ThingDto { ID = 1 },
                new MapCollectionWithEqualityTests.ThingDto { ID = 4 },
                new MapCollectionWithEqualityTests.ThingDto { ID = 2 }
            };

            var a = new MapCollectionWithEqualityTests.Thing { ID = 1 };
            var b = new MapCollectionWithEqualityTests.Thing { ID = 2 };

            // Use a custom IList implementation that lies about Contains in order to skip the pre-add step
            // This forces the mapper to take the list.Insert(i, target) branch when placing the new item
            var destination = new ContainsAlwaysTrueList<MapCollectionWithEqualityTests.Thing> { b, a };

            var result = mapper.Map(source, destination);

            result.Should().BeSameAs(destination);
            result.Select(t => t.ID).Should().ContainInOrder(1, 4, 2);
            result[0].Should().BeSameAs(a);
            result[2].Should().BeSameAs(b);
        }

        // Test double: wraps List<T> but makes Contains return true even when the item is not present
        // This is intentional to exercise the code path that performs Insert(i, target) for new items.
        private class ContainsAlwaysTrueList<T> : IList<T>
        {
            private readonly List<T> _inner = new List<T>();

            public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public void Add(T item) => _inner.Add(item);
            public void Clear() => _inner.Clear();

            // Lie about containment to prevent pre-add of new items
            public bool Contains(T item) => true;

            public void CopyTo(T[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);
            public bool Remove(T item) => _inner.Remove(item);
            public int Count => _inner.Count;
            public bool IsReadOnly => false;
            public int IndexOf(T item) => _inner.IndexOf(item);
            public void Insert(int index, T item) => _inner.Insert(index, item);
            public void RemoveAt(int index) => _inner.RemoveAt(index);
            public T this[int index]
            {
                get => _inner[index];
                set => _inner[index] = value;
            }
        }
    }
}
