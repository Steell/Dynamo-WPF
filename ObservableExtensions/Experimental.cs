using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ObservableExtensions.Experimental
{
    public static class ExperimentalExts
    {
        /// <summary>
        ///     Creates a recursively looping observable, in the form of a feedback loop.
        /// </summary>
        /// <typeparam name="T"/>
        /// <typeparam name="TResult"/>
        /// <param name="seed">Observable sequence used to intialize the loop.</param>
        /// <param name="produce">Projects source values to result values.</param>
        /// <param name="feed">
        ///     Projects result values back to source values to be fed back into the loop.
        /// </param>
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

        /// <summary>
        ///     Creates a recursively loop observable, in the form of a feedback loop.
        /// </summary>
        /// <typeparam name="T"/>
        /// <param name="seed">Observable sequence used to initialize the loop.</param>
        /// <param name="selector">Produces new values to be fed back into the loop.</param>
        public static IObservable<T> Feedback<T>(
            this IObservable<T> seed, Func<T, IObservable<T>> selector)
        {
            return Feedback(seed, selector, Observable.Return);
        }
    }
}
