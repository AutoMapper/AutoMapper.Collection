using System.Linq;
using AutoMapper;
using AutoMapper.EquivilencyExpression;
using AutoMapper.Mappers;

namespace AutoMapperTest.EF7
{
    public class CollectionTestOriginal : CollectionTestBase
    {
        public CollectionTestOriginal()
        {
            Mapper.AddProfile(new CollectionProfile());
        }
    }
}