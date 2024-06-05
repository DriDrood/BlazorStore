namespace DriDrood.BlazorStore.Extensions;
static class EnumerableExtension
{
    public static int MaxOrDefault(this IEnumerable<int> source, int defaultValue = default)
        => source.Any() ? source.Max() : defaultValue;

    public static int IndexOf<T>(this IEnumerable<T> collection, T value)
    {
        int index = 0;
        foreach (T item in collection)
        {
            if (item is null)
            {
                // both is null
                if (value is null)
                    return index;

                continue;
            }

            if (item.Equals(value))
                return index;

            index++;
        }

        return -1;
    }
}