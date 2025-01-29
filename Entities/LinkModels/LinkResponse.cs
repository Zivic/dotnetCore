using Entities.Models;

namespace Entities.LinkModels;

/// <summary>
/// Specifies whether our response includes links.
/// If it does, it will use the LinkedEntities property,
/// otherwise, it will use ShapedEntities.
/// </summary>
public class LinkResponse
{
    public bool HasLinks { get; set; }
    public List<Entity> ShapedEntities { get; set; }
    public LinkCollectionWrapper<Entity> LinkedEntities { get; set; }
    public LinkResponse()
    {
        LinkedEntities = new LinkCollectionWrapper<Entity>();
        ShapedEntities = new List<Entity>();
    }
}