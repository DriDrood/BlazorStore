using DriDrood.BlazorStore.Test.Store;

namespace DriDrood.BlazorStore.Test;

public class StoreGetOrDefault
{
    public StoreGetOrDefault()
    {
        State state = new()
        {
            User = new()
            {
                Name = "Test",
                Age = 18
            },
            Url = "www.example.com",
            Count = 1,
        };

        _store = new(state);
    }

    private TestStore _store;

    [Fact]
    public void GetValue()
    {
        string? url = _store.GetOrDefault(s => s.Url);
        Assert.Equal("www.example.com", url);

        int age = _store.GetOrDefault(s => s.User!.Age);
        Assert.Equal(18, age);

        string? name = _store.GetOrDefault(s => s.User!.Name);
        Assert.Equal("Test", name);
    }

    [Fact]
    public void GetNull()
    {
        string? userName = _store.GetOrDefault(s => s.EmptyUser!.Name);
        Assert.Null(userName);
    }

    [Fact]
    public void ReRender()
    {
        bool isRerendered = false;

        _ = _store.GetOrDefault(s => s.Url, () => isRerendered = true);
        _store.Set(s => s.Url, "www.example2.com");

        Assert.True(isRerendered);
    }

    [Fact]
    public void ReRenderChild()
    {
        bool isRerendered = false;

        _ = _store.GetOrDefault(s => s.User!.Name, () => isRerendered = true);
        _store.Set(s => s.User, new() { Name = "Test2", Age = 18 });

        Assert.True(isRerendered);
    }

    [Fact]
    public void NotReRendered()
    {
        bool isRerendered = false;

        _ = _store.GetOrDefault(s => s.User, () => isRerendered = true);
        _store.Set(s => s.Url, "www.example2.com");

        Assert.False(isRerendered);
    }

    [Fact]
    public void KeyIsLocalVariable()
    {
        string key = "A";
        bool isRerendered = false;
        _ = _store.GetOrDefault(s => s.Dict[key], () => isRerendered = true);

        _store.Set(s => s.Dict[key], "TestAA");

        Assert.True(isRerendered);
    }

    [Fact]
    public void MultipleDependency()
    {
        bool isRerenderedDoubleDependency = false;
        bool isRerenderedFirstValue = false;
        bool isRerenderedSecondValue = false;
        bool isRerenderedOtherDependency = false;
        _ = _store.GetOrDefault(s => s.Dict[s.User!.Name!], () => isRerenderedDoubleDependency = true);
        _ = _store.GetOrDefault(s => s.Dict["Test"], () => isRerenderedFirstValue = true);
        _ = _store.GetOrDefault(s => s.Dict["A"], () => isRerenderedSecondValue = true);
        _ = _store.GetOrDefault(s => s.DictEmpty[s.User!.Name!], () => isRerenderedOtherDependency = true);

        Assert.False(isRerenderedDoubleDependency);
        Assert.False(isRerenderedFirstValue);
        Assert.False(isRerenderedSecondValue);
        Assert.False(isRerenderedOtherDependency);

        _store.Set(s => s.Dict["Test"], "TTT");
        Assert.True(isRerenderedDoubleDependency);
        Assert.True(isRerenderedFirstValue);
        Assert.False(isRerenderedSecondValue);
        Assert.False(isRerenderedOtherDependency);
        isRerenderedDoubleDependency = false;
        isRerenderedFirstValue = false;

        _store.Set(s => s.User!.Name, "A");
        Assert.True(isRerenderedDoubleDependency);
        Assert.False(isRerenderedFirstValue);
        Assert.False(isRerenderedSecondValue);
        Assert.True(isRerenderedOtherDependency);
        isRerenderedDoubleDependency = false;
        isRerenderedOtherDependency = false;

        _store.Set(s => s.Dict["A"], "TTT");
        Assert.True(isRerenderedDoubleDependency);
        Assert.False(isRerenderedFirstValue);
        Assert.True(isRerenderedSecondValue);
        Assert.False(isRerenderedOtherDependency);
    }

    [Fact]
    public void DeepMultipleDependency()
    {
        bool isRerendered = false;
        _ = _store.GetOrDefault(s => s.Dict[s.Dict[s.User!.Name!]], () => isRerendered = true);

        Assert.False(isRerendered);

        _store.Set(s => s.User!.Name, "Test2");
        Assert.True(isRerendered);

        isRerendered = false;
        _store.Set(s => s.Dict["Test"], "TTT");
        Assert.True(isRerendered);
    }
}