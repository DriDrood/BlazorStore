using DriDrood.BlazorStore.Test.Store;

namespace DriDrood.BlazorStore.Test;
public class StoreGet
{
    public StoreGet()
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
        string? url = _store.Get(s => s.Url);
        Assert.Equal("www.example.com", url);

        int age = _store.Get(s => s.User!.Age);
        Assert.Equal(18, age);

        string? name = _store.Get(s => s.User!.Name);
        Assert.Equal("Test", name);
    }

    [Fact]
    public void GetError()
    {
        Assert.Throws<NullReferenceException>(() => _store.Get(s => s.EmptyUser!.Name));
    }

    [Fact]
    public void ReRender()
    {
        bool isRerendered = false;
        Action rerender = () => isRerendered = true;

        _ = _store.Get(s => s.Url, rerender);
        _store.Set(s => s.Url, "www.example2.com");

        Assert.True(isRerendered);
    }

    [Fact]
    public void ReRenderChild()
    {
        bool isRerendered = false;
        Action rerender = () => isRerendered = true;

        _ = _store.Get(s => s.User!.Name, rerender);
        _store.Set(s => s.User, new() { Name = "Test2", Age = 18 });

        Assert.True(isRerendered);
    }

    [Fact]
    public void NotReRendered()
    {
        bool isRerendered = false;
        Action rerender = () => isRerendered = true;

        _ = _store.Get(s => s.User, rerender);
        _store.Set(s => s.Url, "www.example2.com");

        Assert.False(isRerendered);
    }
}