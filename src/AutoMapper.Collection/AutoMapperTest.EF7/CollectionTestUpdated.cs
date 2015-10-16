using AutoMapper;
using AutoMapper.EquivilencyExpression;
using AutoMapper.Mappers;
using System.Linq;

namespace AutoMapperTest.EF7
{
    public class CollectionTestUpdated : CollectionTestBase
    {
        public CollectionTestUpdated()
        {
            Mapper.AddProfile(new CollectionProfileNew());
        }
    }
}