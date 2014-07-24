using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Dynamo.UI
{
    public static class ObservableExtensions
    {
        /// <summary>
        ///     Inverts the values of an observable sequence of bools.
        /// </summary>
        /// <param name="observable">Observable sequence of bools to invert.</param>
        public static IObservable<bool> Not(this IObservable<bool> observable)
        {
            return observable.Select(x => !x);
        }

        /// <summary>
        ///     Projects each element of an observable sequence into consecutive non-overlapping
        ///     buffers containing simultaneous occurrences.
        /// </summary>
        /// <typeparam name="T">Type of elements of the observable sequence.</typeparam>
        /// <param name="observable">An observable sequence.</param>
        public static IObservable<IList<T>> Buffer<T>(this IObservable<T> observable)
        {
            return observable.Buffer(TimeSpan.Zero);
        }
    }
}