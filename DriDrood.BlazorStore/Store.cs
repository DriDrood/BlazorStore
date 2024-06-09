using System.Linq.Expressions;
using DriDrood.BlazorStore.Extensions;
using DriDrood.BlazorStore.Tools;

namespace DriDrood.BlazorStore;
public class Store<TState>
    where TState : notnull, new()
{
    public Store(TState? state = default)
    {
        _state = state ?? new();
        _metadataRoot = new(null, null, null);
    }

    protected TState _state;
    private MetadataNode _metadataRoot;

    public TValue Get<TValue>(Expression<Func<TState, TValue>> expressionPath, Action? componentRerender = null)
    {
        // resolve expression
        ExpressionResolver<TState> resolver = new(_state, _metadataRoot, IndirectDependencyHandling.Write);
        (_, MetadataNode? node) = resolver.Resolve(expressionPath);

        // add dependency
        if (componentRerender is not null && node is not null)
        {
            node.Dependencies.Add(componentRerender);
        }

        // get value
        Func<TState, TValue> getter = expressionPath.Compile();
        TValue value = getter(_state);
        return value;
    }
    public TValue? GetOrDefault<TValue>(Expression<Func<TState, TValue>> expressionPath, Action? componentRerender = null)
    {
        // resolve expression
        ExpressionResolver<TState> resolver = new(_state, _metadataRoot, IndirectDependencyHandling.Write);
        (TValue? value, MetadataNode? node) = resolver.Resolve(expressionPath);

        // add dependency
        if (componentRerender is not null && node is not null)
        {
            node.Dependencies.Add(componentRerender);
            foreach ((MetadataNode indirectParent, MetadataNode mergeNode) in resolver.IndirectParents)
            {
                indirectParent.IndirectDependencies.Add((componentRerender, mergeNode));
            }
        }

        // return value
        return value;
    }

    public void Set<TValue>(Expression<Func<TState, TValue>> expressionPath, TValue newValue)
    {
        // create setter
        ParameterExpression stateParamExpr = expressionPath.Parameters[0];
        Action<TState, TValue> setter = CreateValueSetter(expressionPath, stateParamExpr);

        // change value
        setter(_state, newValue);

        // re-render
        Rerender(expressionPath);
    }

    public void Add<TKey, TValue>(Expression<Func<TState, Dictionary<TKey, TValue>>> expressionPath, TKey key, TValue value)
        where TKey : notnull
    {
        // resolve expression
        ExpressionResolver<TState> resolver = new(_state, _metadataRoot, IndirectDependencyHandling.Write);
        (Dictionary<TKey, TValue>? dict, MetadataNode? node) = resolver.Resolve(expressionPath);
        if (dict is null)
            return;

        // change value
        dict.Add(key, value);

        // re-render
        Rerender(resolver, node, n => n.Dependencies.Concat(n.Children.TryGetValue(key, out var child) ? child.DependencyTree : Enumerable.Empty<Action>()));
    }
    public void Add<TValue>(Expression<Func<TState, ICollection<TValue>>> expressionPath, TValue value)
    {
        // resolve expression
        ExpressionResolver<TState> resolver = new(_state, _metadataRoot, IndirectDependencyHandling.Write);
        (ICollection<TValue>? collection, MetadataNode? node) = resolver.Resolve(expressionPath);
        if (collection is null)
            return;

        // change value
        int valueIndex = collection.Count;
        collection.Add(value);

        // re-render
        Rerender(resolver, node, n => n.Dependencies.Concat(n.Children.TryGetValue(valueIndex, out var child) ? child.DependencyTree : Enumerable.Empty<Action>()));
    }

    public bool Remove<TKey, TValue>(Expression<Func<TState, Dictionary<TKey, TValue>>> expressionPath, TKey key)
        where TKey : notnull
    {
        // resolve expression
        ExpressionResolver<TState> resolver = new(_state, _metadataRoot, IndirectDependencyHandling.Write);
        (Dictionary<TKey, TValue>? dict, MetadataNode? node) = resolver.Resolve(expressionPath);
        if (dict is null)
            return false;

        // change value
        bool result = dict.Remove(key);

        // re-render
        Rerender(resolver, node, n => n.Dependencies.Concat(n.Children.TryGetValue(key, out var child) ? child.DependencyTree : Enumerable.Empty<Action>()));

        return result;
    }
    public bool Remove<TValue>(Expression<Func<TState, ICollection<TValue>>> expressionPath, TValue value)
    {
        // resolve expression
        ExpressionResolver<TState> resolver = new(_state, _metadataRoot, IndirectDependencyHandling.Write);
        (ICollection<TValue>? collection, MetadataNode? node) = resolver.Resolve(expressionPath);
        if (collection is null)
            return false;

        // change value
        int valueIndex = collection.IndexOf(value);
        bool result = collection.Remove(value);

        // re-render
        Rerender(resolver, node, n => n.Dependencies.Concat(n.Children.TryGetValue(valueIndex, out var child) ? child.DependencyTree : Enumerable.Empty<Action>()));

        return result;
    }

    private Action<TState, TValue> CreateValueSetter<TValue>(Expression<Func<TState, TValue>> expr, ParameterExpression stateParamExpr)
    {
        // create lambda value parameter
        ParameterExpression valueParamExpr = Expression.Parameter(typeof(TValue), "value");

        Expression setExpr = (expr.Body is MethodCallExpression callExpr && callExpr.Method.Name == "get_Item")
            // replace get_item with set_item
            ? Expression.Call(callExpr.Object, callExpr.Object!.Type.GetMethod("set_Item")!, [callExpr.Arguments[0], valueParamExpr])
            // assign value
            : Expression.Assign(expr.Body, valueParamExpr);

        // create lambda
        Expression<Action<TState, TValue>> setterExpr = Expression.Lambda<Action<TState, TValue>>(
            setExpr,
            stateParamExpr,
            valueParamExpr);

        return setterExpr.Compile();
    }

    private void Rerender<TValue>(Expression<Func<TState, TValue>> expressionPath, Func<MetadataNode, IEnumerable<Action>>? getDependencies = null)
    {
        ExpressionResolver<TState> resolver = new(_state, _metadataRoot, IndirectDependencyHandling.Read);
        (_, MetadataNode? node) = resolver.Resolve(expressionPath);

        Rerender(resolver, node, getDependencies);
    }
    private void Rerender(ExpressionResolver<TState> resolver, MetadataNode? resolvedNode, Func<MetadataNode, IEnumerable<Action>>? getDependencies = null)
    {
        if (resolvedNode is not null)
        {
            IEnumerable<Action> dependencies = getDependencies?.Invoke(resolvedNode) ?? resolvedNode.DependencyTree;
            foreach (Action componentRerender in dependencies)
            {
                componentRerender();
            }
        }

        foreach (Action componentRerender in resolver.IndirectParents
            .SelectMany(pair => pair.parent.IndirectDependencies
                .Where(depPair => depPair.parent == pair.mergeNode)
                .Select(depPair => depPair.rerenderer)))
        {
            componentRerender();
        }
    }
}
