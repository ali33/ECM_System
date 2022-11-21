using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Ecm.Mvvm
{
    public abstract class BaseDependencyProperty : INotifyPropertyChanged, IDisposable, ICloneable
    {
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real, public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (ThrowOnInvalidPropertyName)
                {
                    throw new Exception(msg);
                }
                else
                {
                    Debug.Fail(msg);
                }
            }
        }

        public void Dispose()
        {
            OnDispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        protected virtual void OnDispose()
        {
        }

#if DEBUG
        ~BaseDependencyProperty()
        {
            string msg = string.Format("{0} ({1}) Finalized", GetType().Name, GetHashCode());
            Debug.WriteLine(msg);
        }
#endif

        #region Implement ICloneable
        public object Clone()
        {
            return OnClone();
        }

        protected virtual object OnClone()
        {
            return this.MemberwiseClone();
        } 
        #endregion
    }
}