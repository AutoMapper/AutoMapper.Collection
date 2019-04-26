using System;

namespace AutoMapper.Collection
{
    public abstract class MappingTestBase
    {
        protected IMapper CreateMapper(Action<IMapperConfigurationExpression> cfg)
        {
            var map = new MapperConfiguration(cfg);
            map.CompileMappings();

            var mapper = map.CreateMapper();
            mapper.ConfigurationProvider.AssertConfigurationIsValid();
            return mapper;
        }
    }
}
