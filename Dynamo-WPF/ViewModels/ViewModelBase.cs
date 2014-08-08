using System;
using System.Reactive.Disposables;

using ReactiveUI;

namespace Dynamo.UI.Wpf.ViewModels
{
    public abstract class ViewModelBase : ReactiveObject, IDisposable
    {
        private readonly CompositeDisposable subscriptions = new CompositeDisposable(); 
        
        public void Dispose()
        {
            subscriptions.Dispose();
        }

        public void RegisterSubscriptionForDisposal(IDisposable subscription)
        {
            subscriptions.Add(subscription);
        }

        public void RegisterSubscriptionsForDisposal(params IDisposable[] subs)
        {
            foreach (var sub in subs)
            {
                subscriptions.Add(sub);
            }
        }
    }
}