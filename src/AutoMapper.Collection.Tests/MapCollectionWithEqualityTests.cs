using System;
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
            Mapper.Reset();
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
            var dtos = new object[100000].Select((_, i) => new ThingDto { ID = i }).ToList();

            var items = new object[100000].Select((_, i) => new Thing { ID = i }).ToList();

            Mapper.Map(dtos, items.ToList()).Should().HaveElementAt(0, items.First());
        }

        public void Should_Be_Fast_With_Large_Reversed_Lists()
        {
            var dtos = new object[100000].Select((_, i) => new ThingDto { ID = i }).ToList();
            dtos.Reverse();

            var items = new object[100000].Select((_, i) => new Thing { ID = i }).ToList();

            Mapper.Map(dtos, items.ToList()).Should().HaveElementAt(0, items.First());
        }

        public void Should_Be_Fast_With_Large_Lists_MultiProperty_Mapping()
        {
            Mapper.Reset();
            Mapper.Initialize(x =>
            {
                x.AddCollectionMappers();
                x.CreateMap<ThingDto, Thing>().EqualityComparison((dto, entity) => dto.ID == entity.ID && dto.ID == entity.ID);
            });

            var dtos = new object[100000].Select((_, i) => new ThingDto { ID = i }).ToList();

            var items = new object[100000].Select((_, i) => new Thing { ID = i }).ToList();

            Mapper.Map(dtos, items.ToList()).Should().HaveElementAt(0, items.First());
        }

        public void Should_Be_Fast_With_Large_Lists_MultiProperty_Mapping_Cant_Extract()
        {
            Mapper.Reset();
            Mapper.Initialize(x =>
            {
                x.AddCollectionMappers();
                x.CreateMap<ThingDto, Thing>().EqualityComparison((dto, entity) => dto.ID == entity.ID || dto.ID == entity.ID);
            });

            var dtos = new object[100000].Select((_, i) => new ThingDto { ID = i }).ToList();

            var items = new object[100000].Select((_, i) => new Thing { ID = i }).ToList();

            Mapper.Map(dtos, items.ToList()).Should().HaveElementAt(0, items.First());
        }

        public void Should_Be_Fast_With_Large_Lists_Cant_Extract_Negative()
        {
            Mapper.Reset();
            Mapper.Initialize(x =>
            {
                x.AddCollectionMappers();
                // ReSharper disable once NegativeEqualityExpression
                x.CreateMap<ThingDto, Thing>().EqualityComparison((dto, entity) => !(dto.ID != entity.ID));
            });

            var dtos = new object[100000].Select((_, i) => new ThingDto { ID = i }).ToList();

            var items = new object[100000].Select((_, i) => new Thing { ID = i }).ToList();

            Mapper.Map(dtos, items.ToList()).Should().HaveElementAt(0, items.First());
        }

        public void Should_Be_Fast_With_Large_Lists_MultiProperty_Mapping_Cant_Extract_Negative()
        {
            Mapper.Reset();
            Mapper.Initialize(x =>
            {
                x.AddCollectionMappers();
                // ReSharper disable once NegativeEqualityExpression
                x.CreateMap<ThingDto, Thing>().EqualityComparison((dto, entity) => dto.ID == entity.ID && !(dto.ID != entity.ID));
            });

            var dtos = new object[100000].Select((_, i) => new ThingDto { ID = i }).ToList();

            var items = new object[100000].Select((_, i) => new Thing { ID = i }).ToList();

            Mapper.Map(dtos, items.ToList()).Should().HaveElementAt(0, items.First());
        }

        public void Should_Be_Fast_With_Large_Lists_SubObject()
        {
            Mapper.Reset();
            Mapper.Initialize(x =>
            {
                x.AddCollectionMappers();
                x.CreateMap<ThingDto, Thing>().EqualityComparison((source, dest) => dest.ID == (source is ThingSubDto ? ((ThingSubDto)source).ID2 : source.ID));
            });

            var dtos = new object[100000].Select((_, i) => new ThingSubDto { ID = i + 100000 }).Cast<ThingDto>().ToList();

            var items = new object[100000].Select((_, i) => new Thing { ID = i }).ToList();

            Mapper.Map(dtos, items.ToList()).Should().HaveElementAt(0, items.First());
        }

        public void Should_Be_Fast_With_Large_Lists_SubObject_WrongCollectionType_Should_Throw()
        {
            Mapper.Reset();
            Mapper.Initialize(x =>
            {
                x.AddCollectionMappers();
                x.CreateMap<ThingDto, Thing>().EqualityComparison((source, dest) => (source is ThingSubDto ? ((ThingSubDto)source).ID2 : source.ID) == dest.ID);
            });

            var dtos = new object[100000].Select((_, i) => new ThingSubDto { ID = i + 100000 }).ToList();

            var items = new object[100000].Select((_, i) => new Thing { ID = i }).ToList();

            Action a = () => Mapper.Map(dtos, items.ToList()).Should().HaveElementAt(0, items.First());
            a.ShouldThrow<ArgumentException>().Where(x => x.Message.Contains(typeof(ThingSubDto).FullName) && x.Message.Contains(typeof(ThingDto).FullName));
        }

        public void Should_Work_With_Conditionals()
        {
            Mapper.Reset();
            Mapper.Initialize(cfg =>
            {
                cfg.AddCollectionMappers();
                cfg.CreateMap<ClientDto, Client>()
                    .EqualityComparison((src, dest) => dest.DtoId == 0 ? src.Code == dest.Code : src.Id == dest.DtoId);
            });
            
            var dto = new ClientDto
            {
                Code = "abc",
                Id = 1
            };
            var entity = new Client {Code = dto.Code, Id = 42};
            var entityCollection = new List<Client> {entity};

            Mapper.Map(new[] {dto}, entityCollection);

            entity.ShouldBeEquivalentTo(entityCollection[0]);
        }

        public class Client
        {
            public long Id { get; set; }
            public string Code { get; set; }
            public long DtoId { get; set; }
        }

        public class ClientDto
        {
            public long Id { get; set; }
            public string Code { get; set; }
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

        public void Should_Work_With_Comparing_String_Types()
        {
            Mapper.Reset();
            Mapper.Initialize(cfg =>
            {
                cfg.AddCollectionMappers();
                cfg.CreateMap<Charge, SaleCharge>()
                    .ForMember(d => d.SaleId, o => o.Ignore())
                    .EqualityComparison((c, sc) => sc.Category == c.Category && sc.Description == c.Description);

                cfg.CreateMap<SaleCharge, Charge>()
                    .ConstructUsing(
                        (saleCharge => new Charge(saleCharge.Category, saleCharge.Description, saleCharge.Value)))
                    .EqualityComparison((sc, c) => sc.Category == c.Category && sc.Description == c.Description);
            });

            var dto = new Charge("catagory", "description", 5);
            var entity = new SaleCharge { Category = dto.Category, Description = dto.Description };
            var entityCollection = new List<SaleCharge> { entity };

            Mapper.Map(new[] { dto }, entityCollection);

            entity.ShouldBeEquivalentTo(entityCollection[0]);
        }

        public class Charge
        {
            public Charge(string category, string description, decimal value)
            {
                Category = category;
                Description = description;
                Value = value;
            }

            public string Category { get; }
            public string Description { get; }
            public decimal Value { get; }

            public override string ToString()
            {
                return $"{Category}|{Description}|{Value}";
            }

            public override int GetHashCode()
            {
                return $"{Category}|{Description}|{Value}".GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                var _obj = obj as Charge;

                if (_obj == null)
                {
                    return false;
                }

                return Category == _obj.Category && Description == _obj.Description && Value == _obj.Value;
            }
        }

        public class SaleCharge
        {
            public Guid SaleId { get; set; }
            public string Category { get; set; }
            public string Description { get; set; }
            public decimal Value { get; set; }
        }

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