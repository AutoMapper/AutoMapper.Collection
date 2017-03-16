namespace AutoMapper.Mappers
{
    public interface IConfigurationObjectMapper : IObjectMapper
    {
        IConfigurationProvider ConfigurationProvider { get; set; }
    }
}