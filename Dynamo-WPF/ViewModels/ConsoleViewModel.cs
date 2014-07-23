using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Dynamo.UI.Models;
using ReactiveUI;

namespace Dynamo.UI.Wpf.ViewModels
{
    public interface IConsoleViewModel
    {
        IEnumerable<LogEntry> Entries { get; } 
        ICommand Clear { get; }
    }

    public class ConsoleViewModel : ReactiveObject, IConsoleViewModel
    {
        public ConsoleViewModel(IObservable<LogEntry> model)
        {
            var entryCollection = model.CreateCollection();
            Entries = entryCollection;

            var clearCommand = ReactiveCommand.Create(entryCollection.IsEmptyChanged.Not());
            clearCommand.Subscribe(_ => entryCollection.Reset());
            Clear = clearCommand;
        }

        public IEnumerable<LogEntry> Entries { get; private set; }
        public ICommand Clear { get; private set; } 
    }

    public class SampleConsoleViewModel : IConsoleViewModel
    {
        public List<LogEntry> SampleEntries
        {
            get { return null; }
            set { Entries = value; }
        }

        public IEnumerable<LogEntry> Entries { get; private set; }

        public ICommand Clear
        {
            get { return null; }
        }
    }

    public static class ObservableExtensions
    {
        public static IObservable<bool> Not(this IObservable<bool> observable)
        {
            return observable.Select(x => !x);
        }

        public static IObservable<U> SampleOther<T, U>(this IObservable<T> observable, IObservable<U> other)
        {
            return
                observable.CombineLatest(other, (t, u) => new { t, u })
                    .DistinctUntilChanged(x => x.t)
                    .Select(x => x.u);
        }
    }
}
