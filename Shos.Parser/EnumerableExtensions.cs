using System;
using System.Globalization;

namespace Shos.Parser;

public static class EnumerableExtensions
{
    public static void ForEach<TElement>(this IEnumerable<TElement> @this, Action<TElement> action)
    {
        foreach (var element in @this)
            action(element);
    }
}
