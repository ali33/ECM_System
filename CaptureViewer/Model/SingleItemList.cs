using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Ecm.CaptureViewer.Model
{
    public class SingleItemList<T> : IList<T> where T : class
    {
        private readonly List<T> _innerList;

        public SingleItemList()
        {
            _innerList = new List<T>();
        }

        public SingleItemList(IEnumerable<T> collection)
        {
            _innerList = new List<T>(collection);
        }

        public SingleItemList(int capacity)
        {
            _innerList = new List<T>(capacity);
        }

        public int IndexOf(T item)
        {
            return _innerList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (_innerList.Contains(item))
            {
                return;
            }

            _innerList.Insert(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void RemoveAt(int index)
        {
            if (_innerList.Count <= index)
            {
                return;
            }

            T removedItem = _innerList[index];
            _innerList.RemoveAt(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem));
        }

        public T this[int index]
        {
            get
            {
                if (index >= _innerList.Count)
                {
                    return null;
                }
                return _innerList[index];
            }
            set
            {
                T oldItem = default(T);
                if (_innerList.Count > index)
                {
                    oldItem = _innerList[index];
                }

                _innerList[index] = value;
                OnCollectionChanged(oldItem != null
                                        ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                                                                               value, oldItem)
                                        : new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));
            }
        }

        public void Add(T item)
        {
            if (_innerList.Contains(item))
            {
                return;
            }

            _innerList.Add(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void Clear()
        {
            var oldItems = new List<T>(_innerList);
            _innerList.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems));
        }

        public bool Contains(T item)
        {
            return _innerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _innerList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            bool status = _innerList.Remove(item);
            if (status)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            }

            return status;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (handler != null) handler(this, e);
        }
    }
}
