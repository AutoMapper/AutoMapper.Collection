using AutoMapper;
using AutoMapper.EquivilencyExpression;
using AutoMapper.Mappers;

namespace AutoMapperTest.EF7
{
    public class CollectionTestOriginal : CollectionTestBase
    {
        public CollectionTestOriginal()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<CollectionProfile>();

                cfg.CreateMap<ProductDto, Product>()
                    .ForMember(p => p.Fields, m => m.MapFrom(p => p.FieldData))
                    ;
                cfg.CreateMap<Product, ProductDto>()
                    .ForMember(p => p.FieldData, m => m.MapFrom(p => p.Fields))
                    ;

                cfg.CreateMap<ProductDto.FieldDataDto, Product.FieldData>()
                    .EqualityComparision((a, b) => a.Id == b.Id)
                    ;
                cfg.CreateMap<Product.FieldData, ProductDto.FieldDataDto>()
                    .EqualityComparision((a, b) => a.Id == b.Id)
                    ;
            });
        }
    }
}