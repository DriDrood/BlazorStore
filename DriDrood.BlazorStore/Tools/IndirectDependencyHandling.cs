namespace DriDrood.BlazorStore.Tools;
public enum IndirectDependencyHandling
{
    /// <summary>
    /// Resolve indirect dependencies by node, default for getting
    /// </summary>
    Write,
    /// <summary>
    /// Resolve indirect dependencies by value, default for setting
    /// </summary>
    Read,
}
