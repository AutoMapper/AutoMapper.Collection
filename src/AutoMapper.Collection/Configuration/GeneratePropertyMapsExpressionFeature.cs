using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.Collection.Execution;
using AutoMapper.EquivalencyExpression;

namespace AutoMapper.Collection.Configuration
{
    public class GeneratePropertyMapsExpressionFeature : IMapperConfigurationExpressionFeature
    {
        private readonly List<Func<Func<Type, object>, IGeneratePropertyMaps>> _generators = new List<Func<Func<Type, object>, IGeneratePropertyMaps>>();

        public void Add(Func<Func<Type, object>, IGeneratePropertyMaps> creator)
        {
            _generators.Add(creator);
        }

        void IMapperConfigurationExpressionFeature.Configure(IConfigurationProvider configurationProvider)
        {
            var generators = _generators
                .Select(x => x.Invoke(configurationProvider.ServiceCtor))
                .ToList();
            configurationProvider.Features.Add(new GeneratePropertyMapsFeature(generators));
        }
    }
}
