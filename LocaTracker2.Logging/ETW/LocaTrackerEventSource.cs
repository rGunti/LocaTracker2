﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocaTracker2.Logging.ETW
{
    [EventSource(Name = "LocaTracker Event Source")]
    public class LocaTrackerEventSource : EventSource
    {
        private LocaTrackerEventSource() { }

        private static LocaTrackerEventSource instance;
        public static LocaTrackerEventSource Instance {
            get {
                if (instance == null) instance = new LocaTrackerEventSource();
                return instance;
            }
        }

        [Event(1, Level = EventLevel.Verbose)]
        public void Verbose(string message) { this.WriteEvent(1, message); }
        public void Verbose(Type source, string message) { Verbose($"SRC={source.Name} - {message}"); }

        [Event(2, Level = EventLevel.Informational)]
        public void Info(string message) { this.WriteEvent(2, message); }
        public void Info(Type source, string message) { Info($"SRC={source.Name} - {message}"); }

        [Event(3, Level = EventLevel.Warning)]
        public void Warn(string message) { this.WriteEvent(3, message); }
        public void Warn(Type source, string message) { Warn($"SRC={source.Name} - {message}"); }

        [Event(4, Level = EventLevel.Error)]
        public void Error(string message) { this.WriteEvent(4, message); }
        public void Error(Type source, string message) { Error($"SRC={source.Name} - {message}"); }

        [Event(5, Level = EventLevel.Critical)]
        public void Critical(string message) { this.WriteEvent(5, message); }
        public void Critical(Type source, string message) { Critical($"SRC={source.Name} - {message}"); }

        [Event(6, Level = EventLevel.LogAlways)]
        public void Always(string message) { this.WriteEvent(6, message); }
        public void Always(Type source, string message) { Always($"SRC={source.Name} - {message}"); }
    }
}
