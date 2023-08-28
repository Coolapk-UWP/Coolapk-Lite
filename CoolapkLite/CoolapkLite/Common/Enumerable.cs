using System;
using System.Collections.Generic;

namespace CoolapkLite.Common
{
    public static class Enumerable
    {
        /// <summary>
        /// Performs the specified action on each element of the <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source"></param>
        /// <param name="action">The <see cref="Action{T}"/> delegate to perform on each element of the <see cref="List{T}"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="action"/> is null.</exception>
        public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (source is List<TSource> list)
            {
                list.ForEach(action);
            }
            else
            {
                foreach (TSource item in source)
                {
                    action(item);
                }
            }
        }

        public static IEnumerable<TResult> GenerateSequence<TResult>(TResult source, Func<TResult, TResult> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            TResult current = source;
            yield return current;

            while (true)
            {
                current = selector(current);
                if (current == null) { break; }
                yield return current;
            }
        }
    }
}
