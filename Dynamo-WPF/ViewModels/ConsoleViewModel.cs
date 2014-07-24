using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Dynamo.UI.Models;
using Microsoft.Expression.Interactivity.Core;
using ReactiveUI;

namespace Dynamo.UI.Wpf.ViewModels
{
    public abstract class AConsoleViewModel : ReactiveObject
    {
        public abstract IEnumerable<LogEntry> Entries { get; }
        public abstract ICommand Clear { get; }

        public string Name
        {
            get { return "Console"; }
        }
    }

    public class ConsoleViewModel : AConsoleViewModel
    {
        public ConsoleViewModel(IObservable<LogEntry> model)
        {
            entryCollection = model.CreateCollection();
            
            clearCommand = ReactiveCommand.Create(entryCollection.IsEmptyChanged.Not());
            clearCommand.Subscribe(_ => entryCollection.Reset());
        }

        private readonly IReactiveDerivedList<LogEntry> entryCollection;  
        public override IEnumerable<LogEntry> Entries { get { return entryCollection; } }

        private readonly ReactiveCommand<object> clearCommand; 
        public override ICommand Clear { get { return clearCommand; } }
    }

    public class SampleConsoleViewModel : AConsoleViewModel
    {
        public List<LogEntry> SampleEntries { get; set; }

        public override IEnumerable<LogEntry> Entries { get { return SampleEntries; } }

        public override ICommand Clear
        {
            get { return new ActionCommand(() => SampleEntries.Clear()); }
        }
    }

    public static class ObservableExtensions
    {
        public static IObservable<bool> Not(this IObservable<bool> observable)
        {
            return observable.Select(x => !x);
        }
    }
}
