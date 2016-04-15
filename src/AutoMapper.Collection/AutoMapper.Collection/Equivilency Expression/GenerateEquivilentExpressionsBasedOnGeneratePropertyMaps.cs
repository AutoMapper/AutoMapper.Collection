using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper.EquivilencyExpression
{
    public abstract class GenerateEquivilentExpressionsBasedOnGeneratePropertyMaps : IGenerateEquivilentExpressions
    {
        private readonly IGeneratePropertyMaps _generatePropertyMaps;
        readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, IGenerateEquivilentExpressions>> _sourceToDestPropMaps = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, IGenerateEquivilentExpressions>>();

        protected GenerateEquivilentExpressionsBasedOnGeneratePropertyMaps(IGeneratePropertyMaps generatePropertyMaps)
        {
            _generatePropertyMaps = generatePropertyMaps;
        }

        public bool CanGenerateEquivilentExpression(Type sourceType, Type destinationType)
        {
            return GetPropertyMatches(sourceType, destinationType) != GenerateEquivilentExpressions.BadValue;
        }

        public IEquivilentExpression GeneratEquivilentExpression(Type sourceType, Type destinationType)
        {
            return GetPropertyMatches(sourceType, destinationType).GeneratEquivilentExpression(sourceType, destinationType);
        }

        private IGenerateEquivilentExpressions GetPropertyMatches(Type sourceType, Type destinationType)
        {
            return _sourceToDestPropMaps
                .GetOrAdd(sourceType, t => new ConcurrentDictionary<Type, IGenerateEquivilentExpressions>())
                .GetOrAdd(destinationType, t =>
                {
                    try
                    {
                        var keyProperties = _generatePropertyMaps.GeneratePropertyMaps(sourceType, destinationType);
                        return keyProperties.Any() ? new GenerateEquivilentExpressionOnPropertyMaps(keyProperties) : GenerateEquivilentExpressions.BadValue;
                    }
                    catch (Exception ex)
                    {
                        return GenerateEquivilentExpressions.BadValue;
                    }
                });
        }
    }
}