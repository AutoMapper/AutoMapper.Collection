using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using AutoMapper.EquivilencyExpression;

namespace AutoMapper.Collection.LinqToSQL
{
    public class GetLinqToSQLPrimaryKeyProperties : IGeneratePropertyMaps
    {
        private readonly IMapper _mapper;

        public GetLinqToSQLPrimaryKeyProperties(IMapper mapper)
        {
            _mapper = mapper;
        }

        public IEnumerable<PropertyMap> GeneratePropertyMaps(Type srcType, Type destType)
        {
            var typeMap = _mapper == null
                ? (Mapper.Configuration as IConfigurationProvider).ResolveTypeMap(srcType, destType)
                : _mapper.ConfigurationProvider.ResolveTypeMap(srcType, destType);
            var propertyMaps = typeMap.GetPropertyMaps();

            var primaryKeyPropertyMatches = destType.GetProperties().Where(IsPrimaryKey).Select(m => propertyMaps.FirstOrDefault(p => p.DestinationProperty.Name == m.Name)).ToList();

            return primaryKeyPropertyMatches;
        }

        private static bool IsPrimaryKey(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes(typeof(ColumnAttribute), false)
                .OfType<ColumnAttribute>().Any(ca => ca.IsPrimaryKey);
        }
    }
}