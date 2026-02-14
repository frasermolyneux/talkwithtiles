using System;
using System.Collections.Generic;

namespace MX.TalkWithTiles.Common.Extensions;

public static class ListExtensions
{
    public static T NextOf<T>(this IList<T> list, T item)
    {
        var indexOf = list.IndexOf(item);
        return list[indexOf == list.Count - 1 ? 0 : indexOf + 1];
    }

    public static T PreviousTo<T>(this IList<T> list, T item)
    {
        var indexOf = list.IndexOf(item);
        return indexOf - 1 >= 0 ? list[indexOf - 1] : list[list.Count - 1];
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = Random.Shared.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}