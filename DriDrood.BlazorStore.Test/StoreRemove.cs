using DriDrood.BlazorStore.Test.Store;

namespace DriDrood.BlazorStore.Test;
public class StoreRemove
{
    public StoreRemove()
    {
        _state = new();
        _store = new(_state);
    }

    private State _state;
    private TestStore _store;

    [Fact]
    public void DictRemove()
    {
        Assert.Contains("A", _state.Dict.Keys);

        _store.Remove(s => s.Dict, "A");

        Assert.DoesNotContain("A", _state.Dict.Keys);
    }

    [Fact]
    public void DictRemoveOutOfRange()
    {
        Assert.Throws<KeyNotFoundException>(() => _store.Remove(s => s.Dict, "B"));
    }

    [Fact]
    public void DictRemoveNull()
    {
        Assert.Throws<ArgumentNullException>(() => _store.Remove(s => s.Dict, null!));
    }

    [Fact]
    public void DictRemoveReRender()
    {
        bool reRenderA = false;
        bool reRenderQ = false;
        
        _ = _store.GetOrDefault(s => s.Dict["A"], () => reRenderA = true);
        _ = _store.GetOrDefault(s => s.Dict["Q"], () => reRenderQ = true);

        _store.Remove(s => s.Dict, "A");

        Assert.True(reRenderA);
        Assert.False(reRenderQ);
    }

    [Fact]
    public void CollectionRemove()
    {
        Assert.Contains("TestA", _state.List);

        _store.Remove(s => s.List, "TestA");

        Assert.DoesNotContain("TestA", _state.List);
    }

    [Fact]
    public void CollectionRemoveOutOfRange()
    {
        Assert.Throws<InvalidOperationException>(() => _store.Remove(s => s.List, "TestB"));
    }
    
    [Fact]
    public void CollectionRemoveReRender()
    {
        bool reRenderA = false;
        bool reRenderQ = false;
        bool reRenderZ = false;
        
        _ = _store.GetOrDefault(s => s.List[0], () => reRenderA = true);
        _ = _store.GetOrDefault(s => s.List[1], () => reRenderQ = true);
        _ = _store.GetOrDefault(s => s.List[2], () => reRenderZ = true);

        _store.Remove(s => s.List, "TestQ");

        Assert.False(reRenderA);
        Assert.True(reRenderQ);
        Assert.False(reRenderZ);
    }
}
