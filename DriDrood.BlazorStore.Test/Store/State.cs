namespace DriDrood.BlazorStore.Test.Store;
internal class State
{
    public User? User { get; set; }
    public User? EmptyUser { get; set; }
    public string? Url { get; set; }
    public int Count { get; set; }
    public Dictionary<string, string> DictEmpty { get; set; } = new();
    public Dictionary<string, string> Dict { get; set; } = new() { { "A", "TestA" }, { "Q", "TestQ" } };
    public List<string> ListEmpty { get; set; } = new();
    public List<string> List { get; set; } = new() { "TestA", "TestQ", "TestZ" };
}