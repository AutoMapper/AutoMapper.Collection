namespace AutoMapper.Mappers
{
    using System.Linq;

    public class CollectionProfile : Profile
    {
        private readonly object _mapperLock = new object();

        protected override void Configure()
        {
            InsertBefore<ReadOnlyCollectionMapper>(new ObjectToEquivalencyExpressionByEquivalencyExistingMapper());
            InsertBefore<ReadOnlyCollectionMapper>(new EquivlentExpressionAddRemoveCollectionMapper());
        }

        private void InsertBefore<TObjectMapper>(IObjectMapper mapper)
            where TObjectMapper : IObjectMapper
        {
            lock (_mapperLock)
            {
                var targetMapper = MapperRegistry.Mappers.FirstOrDefault(om => om is TObjectMapper);
                var index = targetMapper == null ? 0 : MapperRegistry.Mappers.IndexOf(targetMapper);
                MapperRegistry.Mappers.Insert(index, mapper);
            }
        }
    }
}