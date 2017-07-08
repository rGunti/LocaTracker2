using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocaTracker2.Logging
{
    public enum LogLevel
    {
        Verbose = 0,
        Debug   = 1,
        Info    = 2,
        Warning = 3,
        Error   = 4,
        Failure = 5,
        Always  = 6
    }

    public abstract class BaseLogger<T> where T : BaseLogger<T>, new()
    {
        protected static T instance;
        public static T Instance {
            get {
                if (instance == null) instance = new T();
                return instance;
            }
        }

        protected BaseLogger() { InitializeLogger(); }
        
        public bool UseUtcTimes { get; set; } = true;
        public LogLevel MinLogLevel { get; set; }

        protected virtual void InitializeLogger() { }

        protected abstract void ProcessEntry(string entry);

        protected virtual string ComposeMessage(LogLevel level, Type source, string message) =>
            $"{(UseUtcTimes ? DateTime.UtcNow : DateTime.Now):yyyy-MM-dd HH:mm:ss.ffff zzz}|{level,-7}|{source.Name,-32}|{message}";

        public void Log(LogLevel level, object source, string message) => 
            Log(level, source.GetType(), message);
        public void Log(LogLevel level, Type sourceType, string message) {
            if (level >= MinLogLevel)
                ProcessEntry(ComposeMessage(level, sourceType, message));
        }

        public void V(object source, string message) => Log(LogLevel.Verbose, source, message);
        public void V(Type sourceType, string message) => Log(LogLevel.Verbose, sourceType, message);

        public void D(object source, string message) => Log(LogLevel.Debug, source, message);
        public void D(Type sourceType, string message) => Log(LogLevel.Debug, sourceType, message);

        public void I(object source, string message) => Log(LogLevel.Info, source, message);
        public void I(Type sourceType, string message) => Log(LogLevel.Info, sourceType, message);

        public void W(object source, string message) => Log(LogLevel.Warning, source, message);
        public void W(Type sourceType, string message) => Log(LogLevel.Warning, sourceType, message);

        public void E(object source, string message) => Log(LogLevel.Error, source, message);
        public void E(Type sourceType, string message) => Log(LogLevel.Error, sourceType, message);

        public void F(object source, string message) => Log(LogLevel.Failure, source, message);
        public void F(Type sourceType, string message) => Log(LogLevel.Failure, sourceType, message);

        public void A(object source, string message) => Log(LogLevel.Always, source, message);
        public void A(Type sourceType, string message) => Log(LogLevel.Always, sourceType, message);
    }
}
