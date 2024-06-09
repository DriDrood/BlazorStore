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
        bool result = _store.Remove(s => s.Dict, "B");

        Assert.False(result);
    }

    [Fact]
    public void DictRemoveNull()
    {
        Assert.Throws<ArgumentNullException>(() => _store.Remove(s => s.Dict, null!));
    }

    [Fact]
    public void DictRemoveReRender()
    {
        bool reRenderDict = false;
        bool reRenderA = false;
        bool reRenderQ = false;
        
        _ = _store.GetOrDefault(s => s.Dict, () => reRenderDict = true);
        _ = _store.GetOrDefault(s => s.Dict["A"], () => reRenderA = true);
        _ = _store.GetOrDefault(s => s.Dict["Q"], () => reRenderQ = true);

        _store.Remove(s => s.Dict, "A");

        Assert.True(reRenderDict);
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
        bool result = _store.Remove(s => s.List, "TestB");

        Assert.False(result);
    }
    
    [Fact]
    public void CollectionRemoveReRender()
    {
        bool reRenderList = false;
        bool reRenderA = false;
        bool reRenderQ = false;
        bool reRenderZ = false;
        
        _ = _store.GetOrDefault(s => s.List, () => reRenderList = true);
        _ = _store.GetOrDefault(s => s.List[0], () => reRenderA = true);
        _ = _store.GetOrDefault(s => s.List[1], () => reRenderQ = true);
        _ = _store.GetOrDefault(s => s.List[2], () => reRenderZ = true);

        _store.Remove(s => s.List, "TestQ");

        Assert.True(reRenderList);
        Assert.False(reRenderA);
        Assert.True(reRenderQ);
        Assert.False(reRenderZ);
    }
}
