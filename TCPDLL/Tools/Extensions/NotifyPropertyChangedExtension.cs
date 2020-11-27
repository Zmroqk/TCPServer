using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Reflection;

namespace TCPDll.Tools.Extensions
{
    public static class NotifyPropertyChangedExtension
    {
        /// <summary>
        /// Notify that some property has changed value
        /// </summary>
        /// <param name="obj">Instance of INotifyPropertyChanged object</param>
        /// <param name="property">Property name that has changed value</param>
        public static void NotifyPropertyChanged(this INotifyPropertyChanged obj, [CallerMemberName]string property = "")
        {
            PropertyChangedEventHandler handler = (PropertyChangedEventHandler)obj.GetType().GetField("PropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(obj);
            handler?.Invoke(obj, new PropertyChangedEventArgs(property));
        }
    }
}
