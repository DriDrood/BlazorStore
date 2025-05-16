using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace DriDrood.BlazorStore.Tools;
internal class ExpressionResolver<TState>
    where TState : notnull, new()
{
    public ExpressionResolver(TState state, MetadataNode root, IndirectDependencyHandling resolveType)
    {
        _state = state;
        _root = root;
        _resolveType = resolveType;
        IndirectParents = new();
    }

    protected TState _state;
    private MetadataNode _root;
    private IndirectDependencyHandling _resolveType;

    public List<(MetadataNode parent, MetadataNode mergeNode)> IndirectParents { get; }

    public (TValue? outValue, MetadataNode? outNode) Resolve<TValue>(Expression<Func<TState, TValue>> expressionPath)
    {
        (object? result, MetadataNode? node) = ResolveInner(expressionPath.Body);

        return ((TValue?)(result ?? default(TValue)), node);
    }
    public (TValue? outValue, MetadataNode? outNode) ResolveOrNull<TValue>(Expression<Func<TState, TValue>> expressionPath)
        where TValue : struct
    {
        (object? result, MetadataNode? node) = ResolveInner(expressionPath.Body);

        return ((TValue?)(result ?? default), node);
    }

    private (object? outValue, MetadataNode? outNode) ResolveInner(Expression expr)
    {
        // constant
        if (expr is ConstantExpression conExpr)
        {
            return (conExpr.Value, null);
        }

        // parameter
        if (expr is ParameterExpression)
        {
            return (_state, _root);
        }

        // property
        if (expr is MemberExpression memExpr)
        {
            (object? parentValue, MetadataNode? parentNode) = ResolveInner(memExpr.Expression!);
            string propertyName = memExpr.Member.Name;

            object? outValue = memExpr.Member is FieldInfo fieldInfo
                ? fieldInfo.GetValue(parentValue) // local variables
                : parentValue?.GetType().GetProperty(propertyName)!.GetValue(parentValue);
            MetadataNode? outNode = GetOrCreateNode(parentNode, propertyName, expr);

            return (outValue, outNode);
        }

        // []
        if (expr is MethodCallExpression callExpr && callExpr.Method.Name == "get_Item")
        {
            (object? parentValue, MetadataNode? parentNode) = ResolveInner(callExpr.Object!);
            (object? key, MetadataNode? indirectParentNode) = ResolveInner(callExpr.Arguments[0]);

            if (_resolveType == IndirectDependencyHandling.Write && indirectParentNode is not null && parentNode is not null)
            {
                IndirectParents.Add((indirectParentNode, parentNode));
                key = indirectParentNode;
            }
            else if (_resolveType == IndirectDependencyHandling.Read && parentNode is not null)
            {
                foreach ((MetadataNode keyNode, MetadataNode value) in parentNode.IndirectChildren
                    .Where(pair => pair.Key.NodeExpression != expr)
                    .Where(pair => pair.Key.NodeExpression is not null)
                    .ToArray())
                {
                    (object? nodeValue, _) = ResolveInner(keyNode.NodeExpression!);
                    if (nodeValue == key)
                    {
                        IndirectParents.Add((keyNode, parentNode));
                    }
                }
            }

            if (parentValue is null || key is null)
            {
                if (parentValue is null && key is null)
                    return (null, parentNode);
                if (key is not null)
                    return (null, GetOrCreateNode(parentNode, key, expr));

                return (parentValue, parentNode);
            }

            if (parentValue is IDictionary dict)
                return (dict[key], GetOrCreateNode(parentNode, key, expr));

            if (parentValue is IList list)
                return (list[(int)key], GetOrCreateNode(parentNode, key, expr));

            throw new Exception("Not supported collection type");
        }

        // convert implicit/explicit
        if (expr is UnaryExpression unaryExpr)
        {
            if (unaryExpr.NodeType == ExpressionType.Convert)
            {
                return ResolveInner(unaryExpr.Operand);
            }
        }

        // System.Linq.Expressions.BlockExpression
        // System.Linq.Expressions.DebugInfoExpression
        // System.Linq.Expressions.DefaultExpression
        // System.Linq.Expressions.DynamicExpression
        // System.Linq.Expressions.GotoExpression
        // System.Linq.Expressions.IndexExpression
        // System.Linq.Expressions.InvocationExpression
        // System.Linq.Expressions.LabelExpression
        // System.Linq.Expressions.LambdaExpression
        // System.Linq.Expressions.ListInitExpression
        // System.Linq.Expressions.LoopExpression
        // System.Linq.Expressions.MemberInitExpression
        // System.Linq.Expressions.MethodCallExpression
        // System.Linq.Expressions.NewArrayExpression
        // System.Linq.Expressions.NewExpression
        // System.Linq.Expressions.RuntimeVariablesExpression
        // System.Linq.Expressions.SwitchExpression
        // System.Linq.Expressions.TryExpression
        // System.Linq.Expressions.TypeBinaryExpression
        // System.Linq.Expressions.UnaryExpression
        throw new Exception("Not supported expression type");
    }

    private MetadataNode? GetOrCreateNode(MetadataNode? parent, object key, Expression expression)
    {
        if (parent is null)
            return null;

        MetadataNode? childNode;
        if (key is MetadataNode keyNode)
        {
            if (!parent.IndirectChildren.TryGetValue(keyNode, out childNode))
            {
                childNode = new(parent, key, expression);
                parent.IndirectChildren.Add(keyNode, childNode);
            }
        }
        else
        {
            if (!parent.Children.TryGetValue(key, out childNode))
            {
                childNode = new(parent, key, expression);
                parent.Children.Add(key, childNode);
            }
        }

        return childNode;
    }
}