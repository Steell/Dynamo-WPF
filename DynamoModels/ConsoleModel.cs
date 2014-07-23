﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.UI.Models
{
    public struct LogEntry
    {
        public string Text { get; set; }
        public Severity Severity { get; set; }

        public LogEntry(string text, Severity severity) : this()
        {
            Text = text;
            Severity = severity;
        }

        public LogEntry(string text) : this(text, Severity.Info) { }
    }

    public enum Severity
    {
        Info, Hint, Warning, Error
    }
}
