using LocaTracker2.Logging.ETW;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LocaTracker2.Db.Objects
{
    public class BaseNotifyPropertyChange : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            LocaTrackerEventSource.Instance.Verbose($"Property has been changed: {GetType().Name}.{propertyName}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected PropertyInfo GetProperty(string propertyName) { return GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance); }

        public object Get(string propertyName)
        {
            var property = GetProperty(propertyName);
            return property.GetValue(this);
        }

        public void Set(string propertyName, object value)
        {
            var property = GetProperty(propertyName);
            if (null != property && property.CanWrite) {
                property.SetValue(this, value);
            }
        }
    }
}
