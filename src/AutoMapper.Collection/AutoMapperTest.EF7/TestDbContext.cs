using System.Collections.Generic;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;

namespace AutoMapperTest.EF7
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options):base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductDto>(b =>
            {
                b.HasKey(p => p.Id);

                b.HasMany(p => p.FieldData)
                    .WithOne()
                    .WillCascadeOnDelete()
                    .PrincipalKey(x => x.Id)
                    .ForeignKey(x => x.OwnerId).Required();
            });

            modelBuilder.Entity<ProductDto.FieldDataDto>(b =>
            {
                b.HasKey(p => new { p.OwnerId, p.Id });
            });
        }
    }

    public class ProductDto
    {
        public ICollection<FieldDataDto> FieldData { get; set; }
        public string Id { get; set; }

        public class FieldDataDto
        {
            public string OwnerId { get; set; }
            public string Id { get; set; }
            public string Value { get; set; }
        }
    }
}
