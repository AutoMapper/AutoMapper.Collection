using System.Collections.Generic;

namespace AutoMapper.EquivilencyExpression
{
    public interface IGeneratePropertyMaps
    {
        IEnumerable<PropertyMap> GeneratePropertyMaps(TypeMap typeMap);
    }
}