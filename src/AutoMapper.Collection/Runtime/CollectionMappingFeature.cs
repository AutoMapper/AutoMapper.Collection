﻿using AutoMapper.EquivalencyExpression;
using AutoMapper.Features;
using AutoMapper.Internal;

namespace AutoMapper.Collection.Runtime
{
    public class CollectionMappingFeature : IRuntimeFeature
    {
        public CollectionMappingFeature(IEquivalentComparer equivalentComparer)
        {
            EquivalentComparer = equivalentComparer;
        }

        public IEquivalentComparer EquivalentComparer { get; }

        void IRuntimeFeature.Seal(IGlobalConfiguration configurationProvider)
        {
        }
    }
}
