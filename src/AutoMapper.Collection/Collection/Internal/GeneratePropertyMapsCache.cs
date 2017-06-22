using System.Collections.Generic;
using AutoMapper.EquivalencyExpression;

namespace AutoMapper.Collection.Internal
{
    internal static class GeneratePropertyMapsCache
    {
        private static IList<IGeneratePropertyMaps> _generatePropertyMapsCache = new List<IGeneratePropertyMaps>();
        private static readonly IDictionary<IConfigurationProvider, IList<IGeneratePropertyMaps>> _generatePropertyMapsDictionary = new Dictionary<IConfigurationProvider, IList<IGeneratePropertyMaps>>();

        public static void AddGeneratePropertyMaps(IGeneratePropertyMaps generatePropertyMaps)
        {
            _generatePropertyMapsCache.Add(generatePropertyMaps);
        }

        public static IList<IGeneratePropertyMaps> GetGeneratePropertyMaps(IConfigurationProvider configProvider)
        {
            return _generatePropertyMapsDictionary[configProvider];
        }

        internal static void CreatePropertyMaps(IConfigurationProvider configProvider)
        {
            _generatePropertyMapsDictionary.Add(configProvider, _generatePropertyMapsCache);
            _generatePropertyMapsCache = new List<IGeneratePropertyMaps>();
        }
    }
}
