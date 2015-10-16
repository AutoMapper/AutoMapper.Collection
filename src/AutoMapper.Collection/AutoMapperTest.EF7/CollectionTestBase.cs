using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.Data.Entity;
using Xunit;

namespace AutoMapperTest.EF7
{
    public abstract class CollectionTestBase
    {
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