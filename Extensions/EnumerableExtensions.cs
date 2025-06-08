namespace TollFeeCalculator.Extensions;

public static class EnumerableExtensions
{
    public static async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<T, TKey, TValue>(
        this IEnumerable<T> source,
        Func<T, TKey> keySelector,
        Func<T, Task<TValue>> valueSelector) where TKey : notnull
    {
        var result = new Dictionary<TKey, TValue>();
        foreach (var item in source)
        {
            result.Add(keySelector(item), await valueSelector(item));
        }
        return result;
    }
}