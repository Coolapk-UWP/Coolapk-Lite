using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CoolapkLite.Common
{
    public static class Enumerable
    {
        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="ICollection{TSource}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The <see cref="ICollection{TSource}"/> to be added.</param>
        /// <param name="collection">The collection whose elements should be added to the end of the <see cref="ICollection{TSource}"/>.
        /// The collection itself cannot be <see langword="null"/>, but it can contain elements that are
        /// <see langword="null"/>, if type <typeparamref name="TSource"/> is a reference type.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="collection"/> is null.</exception>
        public static void AddRange<TSource>(this ICollection<TSource> source, IEnumerable<TSource> collection)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (source is List<TSource> list)
            {
                list.AddRange(collection);
            }
            else if (source is ISet<TSource> set)
            {
                set.UnionWith(collection);
            }
            else
            {
                foreach (TSource item in collection)
                {
                    source.Add(item);
                }
            }
        }

        /// <summary>
        /// Performs the specified action on each element of the <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source"></param>
        /// <param name="action">The <see cref="Action{T}"/> delegate to perform on each element of the <see cref="List{T}"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="action"/> is null.</exception>
        public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            if (source == null)
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
            else if (source is ImmutableList<TSource> immutableList)
            {
                immutableList.ForEach(action);
            }
            else
            {
                foreach (TSource item in source)
                {
                    action(item);
                }
            }
        }

        /// <summary>
        /// Returns a sequence which invokes the function to calculate the next value
        /// on each iteration until the function returns <see langword="null"/>.
        /// </summary>
        public static IEnumerable<TResult> GenerateSequence<TResult>(Func<TResult> nextFunction)
        {
            if (nextFunction == null)
            {
                throw new ArgumentNullException(nameof(nextFunction));
            }

            while (true)
            {
                TResult current = nextFunction();
                if (current == null) { break; }
                yield return current;
            }
        }

        /// <summary>
        /// Returns a sequence defined by the starting value <paramref name="seed"/> and the function <paramref name="nextFunction"/>,
        /// which is invoked to calculate the next value based on the previous one on each iteration.
        /// </summary>
        public static IEnumerable<TResult> GenerateSequence<TResult>(TResult seed, Func<TResult, TResult> nextFunction)
        {
            if (seed == null)
            {
                throw new ArgumentNullException(nameof(seed));
            }

            if (nextFunction == null)
            {
                throw new ArgumentNullException(nameof(nextFunction));
            }

            TResult current = seed;
            yield return current;

            while (true)
            {
                current = nextFunction(current);
                if (current == null) { break; }
                yield return current;
            }
        }

        /// <summary>
        /// Returns a sequence defined by the function <paramref name="seedFunction"/>, which is invoked to produce the starting value, and the
        /// <paramref name="nextFunction"/>, which is invoked to calculate the next value based on the previous one on each iteration.
        public static IEnumerable<TResult> GenerateSequence<TResult>(Func<TResult> seedFunction, Func<TResult, TResult> nextFunction)
        {
            if (seedFunction == null)
            {
                throw new ArgumentNullException(nameof(seedFunction));
            }

            if (nextFunction == null)
            {
                throw new ArgumentNullException(nameof(nextFunction));
            }

            TResult current = seedFunction();
            yield return current;

            while (true)
            {
                current = nextFunction(current);
                if (current == null) { break; }
                yield return current;
            }
        }

        /// <summary>
        /// Filters the elements of an <see cref="IEnumerable"/> based on a specified type.
        /// </summary>
        /// <typeparam name="TResult">The type to filter the elements of the sequence on.</typeparam>
        /// <param name="source">The <see cref="IEnumerable"/> whose elements to filter.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>An <see cref="IEnumerable{TResult}"/> that contains elements from the input sequence of type <typeparamref name="TResult"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
        public static IEnumerable<TResult> OfType<TResult>(this IEnumerable source, Func<TResult, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            foreach (object obj in source)
            {
                if (obj is TResult result && predicate(result))
                {
                    yield return result;
                }
            }
        }

        /// <summary>
        /// Removes all the elements that match the conditions defined by the specified predicate.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The <see cref="ICollection{TSource}"/> to be removed.</param>
        /// <param name="predicate">The <see cref="Func{T, TResult}"/> delegate that defines the conditions of the elements to remove.</param>
        /// <returns>The number of elements removed from the <see cref="ICollection{TSource}"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
        public static int RemoveAll<TSource>(this ICollection<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (source is List<TSource> list)
            {
                return list.RemoveAll((x) => predicate(x));
            }
            else if (source is HashSet<TSource> hashSet)
            {
                return hashSet.RemoveWhere((x) => predicate(x));
            }
            else if (source is IList<TSource> items)
            {
                int result = 0;
                for (int i = 0; i < items.Count; i++)
                {
                    loop:
                    if (predicate(items[i]))
                    {
                        items.RemoveAt(i);
                        result++;
                        if (i < items.Count) { goto loop; }
                        else { break; }
                    }
                }
                return result;
            }
            else
            {
                return source.RemoveRange(source.Where(predicate).ToArray());
            }
        }

        /// <summary>
        /// Removes the elements of the specified collection of the <see cref="ICollection{TSource}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The <see cref="ICollection{TSource}"/> to be removed.</param>
        /// <param name="collection">The collection whose elements should be removed of the <see cref="ICollection{TSource}"/>.
        /// The collection itself cannot be <see langword="null"/>, but it can contain elements that are
        /// <see langword="null"/>, if type <typeparamref name="TSource"/> is a reference type.</param>
        /// <returns>The number of elements removed from the <see cref="ICollection{TSource}"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="collection"/> is null.</exception>
        public static int RemoveRange<TSource>(this ICollection<TSource> source, IEnumerable<TSource> collection)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            return collection.Select((x) => source.Remove(x)).Count((x) => x);
        }

        /// <summary>
        /// Inverts the order of the elements in a sequence.
        /// </summary>
        /// <param name="text">A sequence of values to reverse.</param>
        /// <returns>A sequence whose elements correspond to those of the input sequence in reverse order.</returns>
        public static string Reverse(this string text)
        {
            char[] charArray = text.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        /// <summary>
        /// Returns a specified number of contiguous elements from the start of a sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The sequence to return elements from.</param>
        /// <param name="count">The number of elements to return.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>An <see cref="IEnumerable{TSource}"/> that contains the specified number of elements from the start of the input sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
        public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            foreach (TSource element in source)
            {
                if (predicate(element))
                {
                    yield return element;
                    if (--count == 0) break;
                }
            }
        }
    }
}
