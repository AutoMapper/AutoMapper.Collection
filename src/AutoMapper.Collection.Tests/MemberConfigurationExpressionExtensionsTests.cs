using System.Collections.Generic;
using Xunit;

namespace AutoMapper.Collection;

public class MemberConfigurationExpressionExtensionsTests : MappingTestBase
{
    
    [Fact]
    public void MapWithOrdinalRecycling_ShouldWorkWithEmptySourceAndDestination()
    {
        var mapper = CreateMapper();
        
        var source = new SourceParent
        {
            Children = new List<SourceChild>()
        };
        
        var destination = new DestinationParent
        {
            Children = new List<DestinationChild>()
        };
        
        mapper.Map(source, destination);
        Assert.Equal(0, destination.Children.Count);
    }
    
    [Fact]
    public void MapWithOrdinalRecycling_ShouldReuseExisting()
    {
        var mapper = CreateMapper();
        
        var source = new SourceParent
        {
            Children = new List<SourceChild>
            {
                new()
                {
                    Property = "Updated Name"
                }
            }
        };
        
        var originalChild = new DestinationChild
        {
            Id = 1,
            Property = "Original Name"
        };
        
        var destination = new DestinationParent
        {
            Children = new List<DestinationChild>
            {
                originalChild
            }
        };
        
        mapper.Map(source, destination);
        Assert.Equal(1, destination.Children.Count);
        Assert.Equal(1, destination.Children[0].Id);
        Assert.Equal("Updated Name", destination.Children[0].Property);
        Assert.True(ReferenceEquals(originalChild, destination.Children[0]));
    }
    
    [Fact]
    public void MapWithOrdinalRecycling_ShouldAddLast()
    {
        var mapper = CreateMapper();
        
        var source = new SourceParent
        {
            Children = new List<SourceChild>
            {
                new()
                {
                    Property = "Item 1 Name"
                },
                new ()
                {
                    Property = "Item 2 Name"
                }
            }
        };
        
        var originalChild = new DestinationChild
        {
            Id = 1,
            Property = "Item 1 Name"
        };
        
        var destination = new DestinationParent
        {
            Children = new List<DestinationChild>
            {
                originalChild
            }
        };
        
        mapper.Map(source, destination);
        Assert.Equal(2, destination.Children.Count);
        Assert.Equal(1, destination.Children[0].Id);
        Assert.Equal("Item 1 Name", destination.Children[0].Property);
        Assert.True(ReferenceEquals(originalChild, destination.Children[0]));
        Assert.Equal("Item 2 Name", destination.Children[1].Property);
    }
    
    [Fact]
    public void MapWithOrdinalRecycling_ShouldRemoveLast()
    {
        var mapper = CreateMapper();
        
        var source = new SourceParent
        {
            Children = new List<SourceChild>
            {
                new()
                {
                    Property = "Item 1 Name"
                }
            }
        };
        
        var originalChild = new DestinationChild
        {
            Id = 1,
            Property = "Item 1 Name"
        };
        
        var destination = new DestinationParent
        {
            Children = new List<DestinationChild>
            {
                originalChild,
                new ()
                {
                    Id = 2,
                    Property = "Item 2 Name"
                }
            }
        };
        
        mapper.Map(source, destination);
        Assert.Equal(1, destination.Children.Count);
        Assert.Equal(1, destination.Children[0].Id);
        Assert.Equal("Item 1 Name", destination.Children[0].Property);
        Assert.True(ReferenceEquals(originalChild, destination.Children[0]));
    }

    private IMapper CreateMapper()
    {
        return CreateMapper(cfg =>
        {
            cfg.CreateMap<SourceParent, DestinationParent>()
                .ForMember(
                    d => d.Children,
                    opt =>
                        opt.MapWithOrdinalRecycling<SourceParent, SourceChild, DestinationParent, DestinationChild>(s =>
                            s.Children));
            cfg
                .CreateMap<SourceChild, DestinationChild>()
                .ForMember(o => o.Id, mo => mo.Ignore());
        });
    }
    
    private class SourceParent
    {
        public List<SourceChild> Children { get; set; } = new();
    }
    
    private class SourceChild
    {
        public string Property { get; init; }
    }
    
    private class DestinationParent
    {
        public IList<DestinationChild> Children { get; set; } = new List<DestinationChild>();
    }
    
    private class DestinationChild
    {
        public int Id { get; set; }
        public string Property { get; set; }
    }
}