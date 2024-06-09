using System.Linq.Expressions;

namespace DriDrood.BlazorStore;
internal class MetadataNode
{
    public MetadataNode(MetadataNode? parent, object? parentKey, Expression? nodeExpression)
    {
        Parent = parent;
        ParentKey = parentKey;
        NodeExpression = nodeExpression;
        Children = new();
        IndirectChildren = new();
        Dependencies = new();
        IndirectDependencies = new();
    }

    public MetadataNode? Parent { get; }
    public object? ParentKey { get; }
    public Expression? NodeExpression { get; }
    public Dictionary<object, MetadataNode> Children { get; }
    public Dictionary<MetadataNode, MetadataNode> IndirectChildren { get; }

    public HashSet<Action> Dependencies { get; }
    public HashSet<(Action rerenderer, MetadataNode parent)> IndirectDependencies { get; }

    public HashSet<Action> DependencyTree => new HashSet<Action>(Children.Values.SelectMany(ch => ch.DependencyTree).Concat(Dependencies).Concat(IndirectDependencies.Select(pair => pair.rerenderer)));

    public override string ToString() => $"{Parent}.{ParentKey}";
}
