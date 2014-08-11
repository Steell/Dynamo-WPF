using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace ObservableExtensions
{
    public static class ObservableExt
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

        /// <summary>
        ///     Samples the source observable sequence using a sampler sequence; whenever the
        ///     sampler produces a value, it is combined with the latest value from the source.
        /// </summary>
        /// <typeparam name="TSample"/>
        /// <typeparam name="TSampler"/>
        /// <typeparam name="TResult"/>
        /// <param name="source">Source observable sequence.</param>
        /// <param name="sampler">Sampler observable sequence.</param>
        /// <param name="selector">Combines values from the source and the sampler.</param>
        public static IObservable<TResult> Sample<TSample, TSampler, TResult>(
            this IObservable<TSample> source,
            IObservable<TSampler> sampler,
            Func<TSample, TSampler, TResult> selector)
        {
            return source.Select(src => sampler.Select(sample => selector(src, sample))).Switch();
        }

        /// <summary>
        ///     Creates a new Observable by repeating the given function forever.
        /// </summary>
        /// <typeparam name="T">Type of elements that the observable sequence produces.</typeparam>
        /// <param name="func">Function used to generate new elements.</param>
        public static IObservable<T> Generate<T>(Func<T> func)
        {
            return Observable.Repeat(func).Select(f => f());
        }

        /// <summary>
        ///     Subscribes to the given observable, displaying its updates to the
        ///     Console.
        /// </summary>
        /// <typeparam name="T">Type of elements of the observable sequence.</typeparam>
        /// <param name="source">Observable sequence to dump.</param>
        /// <param name="name">Label used in the printouts.</param>
        public static IDisposable Dump<T>(this IObservable<T> source, string name)
        {
            return source.SerializeStream(name).Subscribe(Console.WriteLine);
        }

        /// <summary>
        ///     Subscribes to the given observable sequence of strings, displaying
        ///     all elements to the Console.
        /// </summary>
        /// <param name="source">Observable sequence of strings to dump.</param>
        public static IDisposable Dump(this IObservable<string> source)
        {
            return source.Subscribe(Console.WriteLine);
        }

        /// <summary>
        ///     Creates a serialized stream of updates from a given observable sequence.
        /// </summary>
        /// <typeparam name="T"/>
        /// <param name="source">Observable sequence to serialize.</param>
        /// <param name="name">Label used in the output strings.</param>
        public static IObservable<string> SerializeStream<T>(
            this IObservable<T> source, string name)
        {
            return source.Materialize().Select(n => n.NotificationToString(name));
        }

        private static string NotificationToString<T>(
            this Notification<T> notification, string name)
        {
            switch (notification.Kind)
            {
                case NotificationKind.OnNext:
                    return string.Format("{0}-->{1}", name, notification.Value);
                case NotificationKind.OnError:
                    return string.Format("{0} failed-->{1}", name, notification.Exception);
                default:
                    return string.Format("{0} completed", name);
            }
        }
        
        /// <summary>
        ///     Subscribes to either a "true" or "false" stream based on booleans produced by the
        ///     source sequence.
        ///  </summary>
        /// <typeparam name="T"/>
        /// <param name="source">
        ///     Observable sequence of bools. Whenever a value is produced, change subscription to
        ///     the corresponding branch.
        /// </param>
        /// <param name="trueObservable">Observable sequence representing the true branch.</param>
        /// <param name="falseObservable">
        ///     Observable sequence representing the false branch.
        /// </param>
        public static IObservable<T> IfThenElse<T>(
            this IObservable<bool> source, 
            IObservable<T> trueObservable,
            IObservable<T> falseObservable)
        {
            return Observable.Create<T>(obs =>
            {
                var trueBuffered = trueObservable.Replay(1);
                var falseBuffered = falseObservable.Replay(1);

                var branching = 
                    source
                        .Select(testResult => testResult ? trueBuffered : falseBuffered)
                        .Switch();

                return new CompositeDisposable
                {
                    trueBuffered.Connect(),
                    falseBuffered.Connect(),
                    branching.Subscribe(obs)
                };
            });
        }

        /// <summary>
        ///     Waits for a value from the first observable sequence, followed by a value from the
        ///     second observable sequence, then combines them with a given selector function to
        ///     produce new values.
        /// </summary>
        /// <typeparam name="T"/>
        /// <typeparam name="TSource1"/>
        /// <typeparam name="TSource2"/>
        /// <param name="first">Observable sequence to listen to first.</param>
        /// <param name="second">Observable sequence to listen to second.</param>
        /// <param name="selector">
        ///     Function used to combine elements from the two sequences.
        /// </param>
        public static IObservable<IObservable<T>> FollowedBy<T, TSource1, TSource2>(
            this IObservable<TSource1> first,
            IObservable<TSource2> second,
            Func<TSource1, TSource2, T> selector)
        {
            return first.Select(f => second.Select(s => selector(f, s)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="count"></param>
        /// <param name="onlyAtCapacity"></param>
        /// <returns></returns>
        public static IObservable<IEnumerable<T>> BufferOverlap<T>(this IObservable<T> source, int count, bool onlyAtCapacity=false)
        {
            var result = source.Scan(ImmutableQueue<T>.Empty, (queue, arg2) => queue.Enqueue(arg2));
            return onlyAtCapacity ? result.Skip(count - 1) : result;
        }

        /// <summary>
        ///     Sequence of lines input from the Console.
        /// </summary>
        public static IObservable<string> ConsoleLineReader = Generate(Console.ReadLine);
    }
}