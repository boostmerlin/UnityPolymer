using System;
using System.Collections.Generic;
using UniRx;

namespace Ginkgo.UI
{
    public interface IProperty : IPropertyChanged
    {
        ViewModel Owner { get; set; }
        Type ValueType { get; }
        object ObjectValue { get; }
        string PropertyName { get; set; }
    }

    public class P<T> : ISubject<T>, IProperty
    {
        public static readonly EqualityComparer<T> EqualityComparer = EqualityComparer<T>.Default;
        public event PropertyChangedHandler PropertyChanged;

        private T m_objectValue;
        private T m_lastValue;

        public string PropertyName { get; set; }

        public T LastValue
        {
            get { return m_lastValue; }
            set { m_lastValue = value; }
        }
        public ViewModel Owner { get; set; }

        public Type ValueType
        {
            get { return typeof(T); }
        }

        public P(T v)
        {
            m_objectValue = v;
        }
        public P()
        {
            m_objectValue = default(T);
        }

        public static P<T> New(T value, ViewModel owner = null)
        {
            var ret = new P<T>(value);
            ret.Owner = owner;
            return ret;
        }

        public static P<T> New(ViewModel owner = null)
        {
            var ret = new P<T>();
            ret.Owner = owner;
            return ret;
        }

        protected virtual void OnPropertyChanged()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, PropertyName);
            }
            if (Owner != null)
            {
                Owner.OnPropertyChanged(this, PropertyName);
            }
        }

        public T Value
        {
            get { return m_objectValue; }
            set
            {
                if (!EqualityComparer.Equals(value, m_objectValue))
                {
                    m_lastValue = m_objectValue;
                    m_objectValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public object ObjectValue
        {
            get
            {
                return m_objectValue;
            }
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(T value)
        {
            Value = value;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            PropertyChangedHandler handler = delegate
            {
                observer.OnNext(Value);
            };
            PropertyChanged += handler;
            return Disposable.Create(
                () => PropertyChanged -= handler
                );
        }
    }
}
