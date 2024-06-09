using Dumba.BlazorStore.Test.Store;

namespace Dumba.BlazorStore.Test;

public class StoreAdd
{
    public StoreAdd()
    {
        _state = new();
        _store = new(_state);
    }

    private State _state;
    private TestStore _store;

    [Fact]
    public void DictAddValue()
    {
        _store.Add(s => s.DictEmpty, "A", "TestA");

        Assert.Equal("TestA", _state.DictEmpty["A"]);
    }
    
    [Fact]
    public void CollectionAddValue()
    {
        _store.Add(s => s.ListEmpty, "TestA");

        Assert.Contains("TestA", _state.ListEmpty);
    }

    [Fact]
    public void DictExistingKey()
    {
        Assert.Throws<ArgumentException>(() => _store.Add(s => s.Dict, "A", "TestA"));
    }

    [Fact]
    public void ReRenderAdded()
    {
        _store.Add(s => s.ListEmpty, "TestA");

        bool isRendered = false;
        string? value = _store.GetOrDefault(s => s.ListEmpty[0], () => isRendered = true);
        _store.Set(s => s.ListEmpty[0], "TestB");
        string newValue = _state.ListEmpty[0];

        Assert.Equal("TestA", value);
        Assert.Equal("TestB", newValue);
        Assert.True(isRendered);
    }

    [Fact]
    public void ReRenderOnlyAdded()
    {
        _store.Add(s => s.List, "TestB");

        bool isRenderedA = false;
        bool isRenderedB = false;

        string? valueA = _store.GetOrDefault(s => s.List[0], () => isRenderedA = true);
        string? valueB = _store.GetOrDefault(s => s.List[3], () => isRenderedB = true);

        _store.Set(s => s.List[3], "TestC");

        string newValueB = _state.List[3];

        Assert.Equal("TestA", valueA);
        Assert.Equal("TestB", valueB);
        Assert.Equal("TestC", newValueB);
        Assert.False(isRenderedA);
        Assert.True(isRenderedB);
    }
}