using Dumba.BlazorStore.Extensions;
using System.Linq.Expressions;

namespace Dumba.BlazorStore;
public class Store<TState>
    where TState : notnull, new()
{
    public Store(TState? state = default)
    {
        _state = state ?? new();
        _metadataRoot = new MetadataNode(null);
    }

    protected TState _state;
    private MetadataNode _metadataRoot;

    public TValue Get<TValue>(Expression<Func<TState, TValue>> expressionPath, Action? componentRerender = null)
    {
        // add dependency
        if (componentRerender is not null)
        {
            MetadataNode metadataNode = GetNode(expressionPath);
            metadataNode.Dependencies.Add(componentRerender);
        }

        // get value
        Func<TState, TValue> getter = expressionPath.Compile();
        TValue value = getter(_state);
        return value;
    }
    public TValue? GetOrDefault<TValue>(Expression<Func<TState, TValue>> expressionPath, Action? componentRerender = null)
    {
        // add dependency
        if (componentRerender is not null)
        {
            MetadataNode metadataNode = GetNode(expressionPath);
            metadataNode.Dependencies.Add(componentRerender);
        }

        // get value
        TValue? result = expressionPath.GetValueOrNull(_state);
        return result ?? default;
    }

    public void Set<TValue>(Expression<Func<TState, TValue>> expressionPath, TValue value)
    {
        TValue oldValue = Get(expressionPath);

        // change state
        MetadataNode metadataNode = GetNode(expressionPath);
        ParameterExpression stateParamExpr = expressionPath.Parameters[0];

        // set value
        Action<TState, TValue> setter = CreateValueSetter(expressionPath, stateParamExpr);
        setter(_state, value);

        // re-render
        foreach (Action componentRerender in metadataNode.DependencyTree)
        {
            componentRerender();
        }
    }

    public void Add<TKey, TValue>(Expression<Func<TState, Dictionary<TKey, TValue>>> expressionPath, TKey key, TValue value)
        where TKey : notnull
    {
        // create new node
        MetadataNode parentNode = GetNode(expressionPath);
        MetadataNode childNode = new(parentNode);
        parentNode.Children.Add(key, childNode);

        // set value
        Func<TState, Dictionary<TKey, TValue>> getter = expressionPath.Compile();
        Dictionary<TKey, TValue> dict = getter(_state);
        dict.Add(key, value);

        // re-render
        foreach (Action componentRerender in parentNode.DependencyTree)
        {
            componentRerender();
        }
    }
    public void Add<TValue>(Expression<Func<TState, ICollection<TValue>>> expressionPath, TValue value)
    {
        // create new node
        MetadataNode parentNode = GetNode(expressionPath);
        MetadataNode childNode = new(parentNode);
        parentNode.Children.Add(parentNode.Children.Keys.Cast<int>().MaxOrDefault(-1) + 1, childNode);

        // set value
        Func<TState, ICollection<TValue>> getter = expressionPath.Compile();
        ICollection<TValue> collection = getter(_state);
        collection.Add(value);

        // re-render
        foreach (Action componentRerender in parentNode.DependencyTree)
        {
            componentRerender();
        }
    }

    public void Remove<TKey, TValue>(Expression<Func<TState, Dictionary<TKey, TValue>>> expressionPath, TKey key)
        where TKey : notnull
    {
        Func<TState, Dictionary<TKey, TValue>> getter = expressionPath.Compile();
        Dictionary<TKey, TValue> dict = getter(_state);
        TValue oldValue = dict[key];

        // remove node
        MetadataNode parentNode = GetNode(expressionPath);
        MetadataNode childNode = GetOrCreateChild(parentNode, key);
        parentNode.Children.Remove(key);

        // remove value
        dict.Remove(key);

        // re-render
        foreach (Action componentRerender in childNode.DependencyTree)
        {
            componentRerender();
        }
    }
    public void Remove<TValue>(Expression<Func<TState, ICollection<TValue>>> expressionPath, TValue value)
    {
        Func<TState, ICollection<TValue>> getter = expressionPath.Compile();
        ICollection<TValue> collection = getter(_state);

        // remove node
        MetadataNode parentNode = GetNode(expressionPath);
        int index = collection.IndexOf(value);
        if (index == -1)
            index = parentNode.Children.Keys.Cast<int>().Max() + 1;

        MetadataNode childNode = GetOrCreateChild(parentNode, index);
        parentNode.Children.Remove(index);

        // remove value
        collection.Remove(value);

        // re-render
        foreach (Action componentRerender in childNode.DependencyTree)
        {
            componentRerender();
        }
    }

    private MetadataNode GetNode<TValue>(Expression<Func<TState, TValue>> expressionPath)
    {
        IEnumerable<object> path = expressionPath.GetPath();

        MetadataNode result = _metadataRoot;
        foreach (object key in path)
        {
            result = GetOrCreateChild(result, key);
        }

        return result;
    }

    private MetadataNode GetOrCreateChild(MetadataNode parent, object key)
    {
        if (!parent.Children.TryGetValue(key, out MetadataNode? childNode))
        {
            childNode = new(parent);

            parent.Children.Add(key, childNode);
        }

        return childNode;
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
}
