using AutoMapper.EquivilencyExpression;

namespace AutoMapper.Collection.LinqToSQL
{
    public class GetLinqToSQLPrimaryKeyExpression : GenerateEquivilentExpressionsBasedOnGeneratePropertyMaps
    {
        public GetLinqToSQLPrimaryKeyExpression()
            : base(new GetLinqToSQLPrimaryKeyProperties())
        {
        }
    }
}
