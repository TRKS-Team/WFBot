using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WFBot.Utils
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T[]> Permutate<T>(this IEnumerable<T> source)
        {
            return permutate(source, Enumerable.Empty<T>());
            IEnumerable<T[]> permutate(IEnumerable<T> reminder, IEnumerable<T> prefix) =>
                !reminder.Any() ? new[] { prefix.ToArray() } :
                    reminder.SelectMany((c, i) => permutate(
                        reminder.Take(i).Concat(reminder.Skip(i+1)).ToArray(),
                        prefix.Append(c)));
        }
        public static IEnumerable<IEnumerable<T>> DifferentCombinations<T>(this IEnumerable<T> elements, int k)
        {
            var enumerable = elements.ToList();
            return k == 0 ? new[] { Array.Empty<T>() } :
                enumerable.SelectMany((e, i) =>
                    enumerable.Skip(i + 1).DifferentCombinations(k - 1).Select(c => (new[] {e}).Concat(c)));
        }
    }
}
