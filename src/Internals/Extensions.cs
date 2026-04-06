using System;
using System.Collections.Generic;

namespace Maestria.TypeProviders.Internals;

internal static class Extensions
{
    public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
    {
        foreach (T item in enumeration)
        {
            action(item);
        }
    }
}