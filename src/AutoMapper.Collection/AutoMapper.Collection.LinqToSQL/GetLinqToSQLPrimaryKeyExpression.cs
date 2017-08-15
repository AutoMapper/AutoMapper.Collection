using AutoMapper.EquivilencyExpression;

namespace AutoMapper.Collection.LinqToSQL
{
    public class GetLinqToSQLPrimaryKeyExpression : GenerateEquivilentExpressionsBasedOnGeneratePropertyMaps
    {
        /// <summary>
        /// Generate EquivilencyExpressions based on LinqToSQL's primary key
        /// Uses static API's Mapper for finding TypeMap between classes
        /// </summary>
        public GetLinqToSQLPrimaryKeyExpression()
           : base(new GetLinqToSQLPrimaryKeyProperties())
        {
        }
    }
}
