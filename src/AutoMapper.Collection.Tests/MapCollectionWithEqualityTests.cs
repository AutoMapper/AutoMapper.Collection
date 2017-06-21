using System.Collections.Generic;
using System.Linq;
using AutoMapper.EquivalencyExpression;
using FluentAssertions;

namespace AutoMapper.Collection
{
    public class MapCollectionWithEqualityTests
    {
        public MapCollectionWithEqualityTests()
        {
            Mapper.Initialize(x =>
            {
                x.AddCollectionMappers();
                x.CreateMap<ThingDto, Thing>().EqualityComparison((dto, entity) => dto.ID == entity.ID);
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

            Mapper.Map(dtos, items.ToList()).Should().HaveElementAt(0, items.First());
        }

        public void Should_Be_Fast_With_Large_Lists()
        {
            var dtos = new object[100000].Select((_, i) => new ThingDto {ID = i}).ToList();

            var items = new object[100000].Select((_, i) => new Thing { ID = i }).ToList();

            Mapper.Map(dtos, items.ToList()).Should().HaveElementAt(0, items.First());
        }

        public void Should_Be_Fast_With_Large_Lists_GetHashCode()
        {
            Mapper.Initialize(x =>
            {
                x.AddCollectionMappers();
                x.CreateMap<ThingDto, Thing>().EqualityComparison(source => source.ID, dest => dest.ID);
            });

            var dtos = new object[100000].Select((_, i) => new ThingDto {ID = i}).ToList();

            var items = new object[100000].Select((_, i) => new Thing { ID = i }).ToList();

            Mapper.Map(dtos, items.ToList()).Should().HaveElementAt(0, items.First());
        }

        public void Should_Be_Fast_With_Large_Reversed_Lists_GetHashCode()
        {
            Mapper.Initialize(x =>
            {
                x.AddCollectionMappers();
                x.CreateMap<ThingDto, Thing>().EqualityComparison(source => source.ID, dest => dest.ID);
            });

            var dtos = new object[100000].Select((_, i) => new ThingDto {ID = i}).ToList();
            dtos.Reverse();

            var items = new object[100000].Select((_, i) => new Thing { ID = i }).ToList();

            Mapper.Map(dtos, items.ToList()).Should().HaveElementAt(0, items.First());
        }

        public void Should_Be_Fast_With_Large_Lists_GetHashCode_Object()
        {
            Mapper.Initialize(x =>
            {
                x.AddCollectionMappers();
                x.CreateMap<ThingDto, Thing>().EqualityComparison(source => new { source.ID }, dest => new { dest.ID });
            });

            var dtos = new object[100000].Select((_, i) => new ThingDto {ID = i}).ToList();

            var items = new object[100000].Select((_, i) => new Thing { ID = i }).ToList();

            Mapper.Map(dtos, items.ToList()).Should().HaveElementAt(0, items.First());
        }

        public void Should_Be_Fast_With_Large_Lists_GetHashCode_Object2()
        {
            Mapper.Initialize(x =>
            {
                x.AddCollectionMappers();
                x.CreateMap<ThingDto, Thing>().EqualityComparison(source => new { id11 = source.ID, id12 = source.ID }, dest => new { id21 = dest.ID, id22 = dest.ID });
            });

            var dtos = new object[100000].Select((_, i) => new ThingDto {ID = i}).ToList();

            var items = new object[100000].Select((_, i) => new Thing { ID = i }).ToList();

            Mapper.Map(dtos, items.ToList()).Should().HaveElementAt(0, items.First());
        }

        public void Should_Be_Fast_With_Large_Lists_GetHashCode_Object3()
        {
            Mapper.Initialize(x =>
            {
                x.AddCollectionMappers();
                x.CreateMap<ThingDto, Thing>().EqualityComparison(source => new { id11 = source is ThingSubDto ? ((ThingSubDto)source).ID2 : source.ID }, dest => new { id21 = dest.ID });
            });

            var dtos = new object[100000].Select((_, i) => new ThingSubDto {ID = i + 100000}).ToList();

            var items = new object[100000].Select((_, i) => new Thing { ID = i }).ToList();

            Mapper.Map(dtos, items.ToList()).Should().HaveElementAt(0, items.First());
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

        public void Should_Be_Instanced_Based()
        {
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

            Mapper.Map(dtos, items.ToList()).Should().NotContain(items.First());
        }

        public void Parent_Should_Be_Same_As_Root_Object()
        {
            var mapper = new MapperConfiguration(
                cfg =>
                {
                    cfg.AddCollectionMappers();
                    cfg.CreateMap<ThingWithCollection, ThingWithCollection>()
                       .PreserveReferences();
                    cfg.CreateMap<ThingCollectionItem, ThingCollectionItem>()
                       .EqualityComparison((src, dst) => src.ID == dst.ID)
                       .PreserveReferences();
                })
                .CreateMapper();

            var root = new ThingWithCollection()
            {
                Children = new List<ThingCollectionItem>()
            };
            root.Children.Add(new ThingCollectionItem() { ID = 1, Parent = root });

            var target = new ThingWithCollection() { Children = new List<ThingCollectionItem>() };
            mapper.Map(root, target).Should().Be(target);

            target.Children.Count.Should().Be(1);
            target.Children.Single().Parent.Should().Be(target);
        }

        public class Thing
        {
            public int ID { get; set; }
            public string Title { get; set; }
            public override string ToString() { return Title; }
        }

        public class ThingSubDto : ThingDto
        {
            public int ID2 => ID - 100000;
        }

        public class ThingDto
        {
            public int ID { get; set; }
            public string Title { get; set; }
        }

        public class ThingWithCollection
        {
            public ICollection<ThingCollectionItem> Children { get; set; }
        }

        public class ThingCollectionItem
        {
            public int ID { get; set; }
            public ThingWithCollection Parent { get; set; }
        }
    }
}
