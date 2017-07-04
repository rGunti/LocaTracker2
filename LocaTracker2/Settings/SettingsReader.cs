using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LocaTracker2.Settings
{
    public abstract class SettingsReader<T> where T : SettingsReader<T>, new()
    {
        private static T instance;
        public static T Instance {
            get {
                if (instance == null) instance = new T();
                return instance;
            }
        }

        protected SettingsReader() { InitializeSettingsValues(); }

        protected abstract void InitializeSettingsValues();

        protected void InitSettingsValue(string key, object defaultValue) { if (!HasKey(key)) SetValue(key, defaultValue); }

        protected bool HasKey(string key) { return ApplicationData.Current.LocalSettings.Values.ContainsKey(key); }
        protected object GetValue(string key) { return ApplicationData.Current.LocalSettings.Values[key]; }
        protected void SetValue(string key, object value) { ApplicationData.Current.LocalSettings.Values[key] = value; }

        protected T GetValue<T>(string key)
        {
            object val = GetValue(key);
            if (val != null && val is T) return (T)val;
            else throw new InvalidCastException($"A settings value with key {key} has been requested as type {typeof(T)} but is of another type or null.");
        }

        protected bool GetBool(string key) { return GetValue<bool>(key); }
        protected void SetBool(string key, bool value) { SetValue(key, value); }
    }
}
