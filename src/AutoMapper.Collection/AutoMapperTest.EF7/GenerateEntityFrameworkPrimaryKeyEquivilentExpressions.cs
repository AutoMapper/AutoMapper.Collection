
using AutoMapper.EquivilencyExpression;
using Microsoft.Data.Entity;

namespace AutoMapper.EntityFramework
{
    /// <summary>
    /// Generates equivilency expressions for Types mapping to entities that are part of <typeparamref name="TDatabaseContext"/>'s model
    /// </summary>
    /// <typeparam name="TDatabaseContext">Database Context</typeparam>
    public class GenerateEntityFrameworkPrimaryKeyEquivilentExpressions<TDatabaseContext> : GenerateEquivilentExpressionsBasedOnGeneratePropertyMaps
        where TDatabaseContext : DbContext, new() 
    {
        public GenerateEntityFrameworkPrimaryKeyEquivilentExpressions()
            : base(new GenerateEntityFrameworkPrimaryKeyPropertyMaps<TDatabaseContext>())
        {
        }
    }
}