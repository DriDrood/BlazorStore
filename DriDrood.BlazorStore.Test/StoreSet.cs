using DriDrood.BlazorStore.Test.Store;

namespace DriDrood.BlazorStore.Test;

public class StoreSet
{
    public StoreSet()
    {
        _state = new()
        {
            User = new()
            {
                Name = "Test",
                Age = 18
            },
            Url = "www.example.com",
            Count = 1,
        };

        _store = new(_state);
    }

    private State _state;
    private TestStore _store;

    [Fact]
    public void SetValue()
    {
        _store.Set(s => s.Url, "www.example2.com");
        Assert.Equal("www.example2.com", _state.Url);

        _store.Set(s => s.User!.Age, 19);
        Assert.Equal(19, _state.User!.Age);

        _store.Set(s => s.User!.Name, "Test2");
        Assert.Equal("Test2", _state.User!.Name);

        _store.Set(s => s.Dict["BBB"], "abc");
        Assert.Equal("abc", _state.Dict["BBB"]);
    }

    [Fact]
    public void SetDictValue()
    {
        _store.Set(s => s.Dict, "A", "TestB");
        Assert.Equal("TestB", _state.Dict["A"]);

        _store.Set(s => s.Dict, "uuu", "TestC");
        Assert.True(_state.Dict.ContainsKey("uuu"));
        Assert.Equal("TestC", _state.Dict["uuu"]);
    }
}