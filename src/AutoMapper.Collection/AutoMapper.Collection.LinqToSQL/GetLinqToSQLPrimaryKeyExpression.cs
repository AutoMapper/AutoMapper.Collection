using AutoMapper.EquivilencyExpression;

namespace AutoMapper.Collection.LinqToSQL
{
    public class GetLinqToSQLPrimaryKeyExpression : GenerateEquivilentExpressionsBasedOnGeneratePropertyMaps
    {
        /// <summary>
        /// Generate EquivilencyExpressions based on LinqToSQL's primary key
        /// </summary>
        /// <param name="mapper">IMapper used to find TypeMap between classes</param>
        public GetLinqToSQLPrimaryKeyExpression(IMapper mapper)
            : base(new GetLinqToSQLPrimaryKeyProperties(mapper))
        {
        }

        /// <summary>
        /// Generate EquivilencyExpressions based on LinqToSQL's primary key
        /// Uses static API's Mapper for finding TypeMap between classes
        /// </summary>
        public GetLinqToSQLPrimaryKeyExpression()
           : base(new GetLinqToSQLPrimaryKeyProperties(null))
        {
        }
    }
}
