namespace ZD_Article_Grabber.Interfaces
{
    public interface IResourceInstructions
    {
        bool IsResourceMatched(Types.ResourceType reousrceType);
        IReadOnlyDictionary<Types.ResourceType, Types.Instructions> Instructions { get; }
    }
}
