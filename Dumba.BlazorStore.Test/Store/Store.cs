namespace Dumba.BlazorStore.Test.Store;
internal class TestStore : Store<State>
{
    public TestStore(State? state = null) : base(state)
    {
    }
}
