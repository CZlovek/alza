namespace Tests;

/// <summary>
/// Provides a set of extension methods for handling asynchronous enumerable operations.
/// </summary>
public static class Extensions
{
    /// Converts an IEnumerable<T> into an IAsyncEnumerable<T>.
    /// This method is useful for adapting synchronous collections to asynchronous streaming contexts by yielding each item asynchronously.
    /// <param name="source">The source IEnumerable<T> to be converted to an asynchronous enumerable.</param>
    /// <typeparam name="T">The type of the elements in the source collection.</typeparam>
    /// <returns>An asynchronous enumerable representation of the source collection.</returns>
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            yield return item;
            await Task.Yield();
        }
    }

    /// Converts an asynchronous enumerable sequence into a List asynchronously.
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The IAsyncEnumerable to convert into a List.</param>
    /// <returns>A Task representing the asynchronous operation. The result contains a List with all elements from the source sequence.</returns>
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        var result = new List<T>();

        var enumerator = source.GetAsyncEnumerator();

        while(await enumerator.MoveNextAsync())
        {
            result.Add(enumerator.Current);
        }

        return result;
    }
}