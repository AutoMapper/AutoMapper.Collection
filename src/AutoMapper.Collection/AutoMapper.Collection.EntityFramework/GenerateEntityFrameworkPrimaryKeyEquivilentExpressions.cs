using System.Data.Entity.Infrastructure;
using AutoMapper.EquivilencyExpression;

namespace AutoMapper.EntityFramework
{
    /// <summary>
    /// Generates equivilency expressions for Types mapping to entities that are part of <typeparamref name="TDatabaseContext"/>'s model
    /// </summary>
    /// <typeparam name="TDatabaseContext">Database Context</typeparam>
    public class GenerateEntityFrameworkPrimaryKeyEquivilentExpressions<TDatabaseContext> : GenerateEquivilentExpressionsBasedOnGeneratePropertyMaps
        where TDatabaseContext : IObjectContextAdapter, new()
    {

        /// <summary>
        /// Generate EquivilencyExpressions based on EnityFramework's primary key
        /// Uses static API's Mapper for finding TypeMap between classes
        /// </summary>
        public GenerateEntityFrameworkPrimaryKeyEquivilentExpressions()
           : base(new GenerateEntityFrameworkPrimaryKeyPropertyMaps<TDatabaseContext>())
        {
        }
    }
}