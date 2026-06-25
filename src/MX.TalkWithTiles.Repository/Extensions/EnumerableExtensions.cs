using System.Collections.Generic;

namespace MX.TalkWithTiles.Repository.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> collection, int batchSize)
    {
        var nextBatch = new List<T>(batchSize);
        foreach (var item in collection)
        {
            nextBatch.Add(item);
            if (nextBatch.Count == batchSize)
            {
                yield return nextBatch;
                nextBatch = new List<T>(batchSize);
            }
        }

        if (nextBatch.Count > 0)
        {
            yield return nextBatch;
        }
    }
}