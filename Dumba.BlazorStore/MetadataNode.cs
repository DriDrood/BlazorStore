namespace Dumba.BlazorStore;
internal class MetadataNode
{
    public MetadataNode(MetadataNode? parent)
    {
        Parent = parent;
        Children = new();
        Dependencies = new();
    }

    public MetadataNode? Parent { get; }
    public Dictionary<object, MetadataNode> Children { get; }

    public HashSet<Action> Dependencies { get; }

    public HashSet<Action> DependencyTree => new HashSet<Action>(Children.Values.SelectMany(ch => ch.DependencyTree).Concat(Dependencies));
}
