using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

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
        /// 
        /// </summary>
        /// <typeparam name="TSample"></typeparam>
        /// <typeparam name="TSampler"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="sampler"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IObservable<TResult> Sample<TSample, TSampler, TResult>(this IObservable<TSample> source,
            IObservable<TSampler> sampler, Func<TSample, TSampler, TResult> selector)
        {
            return source.Sample(sampler).Zip(sampler, selector);
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
        /// <typeparam name="T">Type of elements of the observable sequence.</typeparam>
        /// <param name="source">Observable sequence to serialize.</param>
        /// <param name="name">Label used in the output strings.</param>
        public static IObservable<string> SerializeStream<T>(this IObservable<T> source, string name)
        {
            return source.Materialize().Select(n => n.NotificationToString(name));
        }

        private static string NotificationToString<T>(this Notification<T> notification, string name)
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

        public static IObservable<TResult> Feedback<T, TResult>(
            this IObservable<T> seed,
            Func<T, IObservable<TResult>> produce,
            Func<TResult, IObservable<T>> feed)
        {
            return Observable.Create<TResult>(
                obs =>
                {
                    var disposable = new CompositeDisposable();

                    var seedPublished = seed.Publish();
                    var feedbackSubject = new Subject<T>();
                    var resultStream = feedbackSubject.SelectMany(produce);

                    var feedbackSubscription =
                        resultStream
                            .SelectMany(feed)
                            .Subscribe(feedbackSubject.OnNext);

                    var resultSubscription = resultStream.Subscribe(
                        obs.OnNext,
                        obs.OnError,
                        () =>
                        {
                            disposable.Dispose();
                            obs.OnCompleted();
                        });

                    var seedS = seedPublished.Subscribe(feedbackSubject);
                    var seedSubscription = seedPublished.Connect();

                    disposable.Add(feedbackSubscription);
                    disposable.Add(feedbackSubject);
                    disposable.Add(resultSubscription);
                    disposable.Add(seedSubscription);
                    disposable.Add(seedS);

                    return disposable;
                });
        }

        public static IObservable<T> Feedback<T>(this IObservable<T> seed, Func<T, IObservable<T>> selector)
        {
            return Feedback(seed, selector, Observable.Return);
        }

        public static IObservable<T> IfThenElse<T>(
            this IObservable<bool> testObservable, 
            IObservable<T> trueObsservable,
            IObservable<T> falseObservable)
        {
            return Observable.Create<T>(obs =>
            {
                var trueBuffered = trueObsservable.Replay(1);
                var falseBuffered = falseObservable.Replay(1);

                var branching = testObservable.Select(testResult => testResult ? trueBuffered : falseBuffered).Switch();

                return new CompositeDisposable
                {
                    trueBuffered.Connect(),
                    falseBuffered.Connect(),
                    branching.Subscribe(obs)
                };
            });
        }

        /// <summary>
        ///     Sequence of lines input from the Console.
        /// </summary>
        public static IObservable<string> ConsoleLineReader = Generate(Console.ReadLine);
    }
}