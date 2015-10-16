using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.EntityFramework;
using Microsoft.Data.Entity;
using Xunit;
using AutoMapper.EquivilencyExpression;
using AutoMapper.Mappers;

namespace AutoMapperTest.EF7
{
    public abstract class CollectionTestBase
    {
        protected CollectionTestBase()
        {
            EquivilentExpressions.GenerateEquality.Add(new GenerateEntityFrameworkPrimaryKeyEquivilentExpressions<TestDbContext>());
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<ProductDto, Product>()
                    .ForMember(p => p.Fields, m => m.MapFrom(p => p.FieldData))
                    ;
                cfg.CreateMap<Product, ProductDto>()
                    .ForMember(p => p.FieldData, m => m.MapFrom(p => p.Fields))
                    ;

                cfg.CreateMap<ProductDto.FieldDataDto, Product.FieldData>();
                cfg.CreateMap<Product.FieldData, ProductDto.FieldDataDto>();
            });
        }

        protected void InsertBefore<TObjectMapper>(IObjectMapper mapper)
            where TObjectMapper : IObjectMapper
        {
            var targetMapper = MapperRegistry.Mappers.FirstOrDefault(om => om is TObjectMapper);
            var index = targetMapper == null ? 0 : MapperRegistry.Mappers.IndexOf(targetMapper);
            MapperRegistry.Mappers.Insert(index, mapper);
        }


        [Fact]
        public void TestChildCollectionMapping()
        {
            var id = "test1";
            var options = new DbContextOptionsBuilder<TestDbContext>();
            options.UseInMemoryDatabase();

            using (var db = new TestDbContext(options.Options))
            {
                var a = new Product
                {
                    Id = id,
                    Fields = new List<Product.FieldData>
                    {
                        new Product.FieldData {Id = "field1", Value = "1"},
                        new Product.FieldData {Id = "field2", Value = "2"},
                    }
                };

                var dtoA = Mapper.Map(a, new ProductDto());
                db.Set<ProductDto>().Add(dtoA);
                db.SaveChanges();
            }

            Product b;
            using (var db = new TestDbContext(options.Options))
            {
                var dtoB = db.Set<ProductDto>().Include(x => x.FieldData).FirstOrDefault(x => x.Id == id);
                b = Mapper.Map<Product>(dtoB);
            }

            b.Fields.Single(x => x.Id == "field1").Value = "1-updated";

            using (var db = new TestDbContext(options.Options))
            {
                var dtoB = db.Set<ProductDto>().Include(x => x.FieldData).FirstOrDefault(x => x.Id == id);

                Mapper.Map(b, dtoB);

                db.SaveChanges();
            }
        }

        public class Product
        {
            public ICollection<FieldData> Fields { get; set; }
            public string Id { get; set; }

            public class FieldData
            {
                public string Id { get; set; }
                public string Value { get; set; }
            }
        }

    }
}