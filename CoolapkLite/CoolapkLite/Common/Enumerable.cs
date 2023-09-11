﻿using System;
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
