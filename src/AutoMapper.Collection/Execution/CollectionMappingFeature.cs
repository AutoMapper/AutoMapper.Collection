using AutoMapper.EquivalencyExpression;

namespace AutoMapper.Collection.Execution
{
    public class CollectionMappingFeature : IFeature
    {
        public CollectionMappingFeature(IEquivalentComparer equivalentComparer)
        {
            EquivalentComparer = equivalentComparer;
        }

        public IEquivalentComparer EquivalentComparer { get; }

        void IFeature.Seal(IConfigurationProvider configurationProvider)
        {
        }
    }
}
