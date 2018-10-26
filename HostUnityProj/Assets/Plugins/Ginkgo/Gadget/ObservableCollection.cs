// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@novell.com)
//	Brian O'Keefe (zer0keefie@gmail.com)
//
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Collections;

namespace Ginkgo
{
    #region CollectionChangedEventArgs
    public enum ChangedAction
    {
        Reset,
        Add,
        Move,
        Remove,
        Replace
    }
    public class CollectionChangedEventArgs : EventArgs
    {
        #region " Attributes "
        private ChangedAction _notifyAction;
        private IList _newItemList;
        private int _newStartingIndex;
        private IList _oldItemList;
        private int _oldStartingIndex;
        #endregion

        #region " Constructors "
        public CollectionChangedEventArgs(ChangedAction action)
        {
            this._newStartingIndex = -1;
            this._oldStartingIndex = -1;
            if (action != ChangedAction.Reset)
            {
                throw new ArgumentException("Wrong Action For Ctor", "action");
            }
            this.InitializeAdd(action, null, -1);
        }

        public CollectionChangedEventArgs(ChangedAction action, IList changedItems)
        {
            this._newStartingIndex = -1;
            this._oldStartingIndex = -1;
            if (((action != ChangedAction.Add) && (action != ChangedAction.Remove)) && (action != ChangedAction.Reset))
            {
                throw new ArgumentException("Must Be Reset Add Or Remove Action For Ctor", "action");
            }
            if (action == ChangedAction.Reset)
            {
                if (changedItems != null)
                {
                    throw new ArgumentException("Reset Action Requires Null Item", "action");
                }
                this.InitializeAdd(action, null, -1);
            }
            else
            {
                if (changedItems == null)
                {
                    throw new ArgumentNullException("changed Items");
                }
                this.InitializeAddOrRemove(action, changedItems, -1);
            }
        }

        public CollectionChangedEventArgs(ChangedAction action, Object changedItem)
        {
            this._newStartingIndex = -1;
            this._oldStartingIndex = -1;
            if (((action != ChangedAction.Add) && (action != ChangedAction.Remove)) && (action != ChangedAction.Reset))
            {
                throw new ArgumentException("Must Be Reset Add Or Remove Action For Ctor", "action");
            }
            if (action == ChangedAction.Reset)
            {
                if (changedItem != null)
                {
                    throw new ArgumentException("Reset Action Requires Null Item", "action");
                }
                this.InitializeAdd(action, null, -1);
            }
            else
            {
                this.InitializeAddOrRemove(action, new object[] { changedItem }, -1);
            }
        }


        public CollectionChangedEventArgs(ChangedAction action, IList newItems, IList oldItems)
        {
            this._newStartingIndex = -1;
            this._oldStartingIndex = -1;
            if (action != ChangedAction.Replace)
            {
                throw new ArgumentException("Wrong Action For Ctor", "action");
            }
            if (newItems == null)
            {
                throw new ArgumentNullException("new Items");
            }
            if (oldItems == null)
            {
                throw new ArgumentNullException("old Items");
            }
            this.InitializeMoveOrReplace(action, newItems, oldItems, -1, -1);
        }

        public CollectionChangedEventArgs(ChangedAction action, IList changedItems, int startingIndex)
        {
            this._newStartingIndex = -1;
            this._oldStartingIndex = -1;
            if (((action != ChangedAction.Add) && (action != ChangedAction.Remove)) && (action != ChangedAction.Reset))
            {
                throw new ArgumentException("Must Be Reset Add Or Remove Action For Ctor", "action");
            }
            if (action == ChangedAction.Reset)
            {
                if (changedItems != null)
                {
                    throw new ArgumentException("Reset Action Requires Null Item", "action");
                }
                if (startingIndex != -1)
                {
                    throw new ArgumentException("Reset Action Requires Index Minus 1", "action");
                }
                this.InitializeAdd(action, null, -1);
            }
            else
            {
                if (changedItems == null)
                {
                    throw new ArgumentNullException("changed Items");
                }
                if (startingIndex < -1)
                {
                    throw new ArgumentException("Index Cannot Be Negative", "startingIndex");
                }
                this.InitializeAddOrRemove(action, changedItems, startingIndex);
            }
        }

        public CollectionChangedEventArgs(ChangedAction action, Object changedItem, int index)
        {
            this._newStartingIndex = -1;
            this._oldStartingIndex = -1;
            if (((action != ChangedAction.Add) && (action != ChangedAction.Remove)) && (action != ChangedAction.Reset))
            {
                throw new ArgumentException("Must Be Reset Add Or Remove Action For Ctor", "action");
            }
            if (action == ChangedAction.Reset)
            {
                if (changedItem != null)
                {
                    throw new ArgumentException("Reset Action Requires Null Item", "action");
                }
                if (index != -1)
                {
                    throw new ArgumentException("Reset Action Requires Index Minus 1", "action");
                }
                this.InitializeAdd(action, null, -1);
            }
            else
            {
                this.InitializeAddOrRemove(action, new object[] { changedItem }, index);
            }
        }

        public CollectionChangedEventArgs(ChangedAction action, Object newItem, Object oldItem)
        {
            this._newStartingIndex = -1;
            this._oldStartingIndex = -1;
            if (action != ChangedAction.Replace)
            {
                throw new ArgumentException("Wrong Action For Ctor", "action");
            }
            this.InitializeMoveOrReplace(action, new object[] { newItem }, new object[] { oldItem }, -1, -1);
        }

        public CollectionChangedEventArgs(ChangedAction action, IList newItems, IList oldItems, int startingIndex)
        {
            this._newStartingIndex = -1;
            this._oldStartingIndex = -1;
            if (action != ChangedAction.Replace)
            {
                throw new ArgumentException("Wrong Action For Ctor", "action");
            }
            if (newItems == null)
            {
                throw new ArgumentNullException("new Items");
            }
            if (oldItems == null)
            {
                throw new ArgumentNullException("old Items");
            }
            this.InitializeMoveOrReplace(action, newItems, oldItems, startingIndex, startingIndex);
        }

        public CollectionChangedEventArgs(ChangedAction action, IList changedItems, int index, int oldIndex)
        {
            this._newStartingIndex = -1;
            this._oldStartingIndex = -1;
            if (action != ChangedAction.Move)
            {
                throw new ArgumentException("Wrong Action For Ctor", "action");
            }
            if (index < 0)
            {
                throw new ArgumentException("Index Cannot Be Negative", "index");
            }
            this.InitializeMoveOrReplace(action, changedItems, changedItems, index, oldIndex);
        }

        public CollectionChangedEventArgs(ChangedAction action, Object changedItem, int index, int oldIndex)
        {
            this._newStartingIndex = -1;
            this._oldStartingIndex = -1;
            if (action != ChangedAction.Move)
            {
                throw new ArgumentException("Wrong Action For Ctor", "action");
            }
            if (index < 0)
            {
                throw new ArgumentException("Index Cannot Be Negative", "index");
            }
            object[] newItems = new object[] { changedItem };
            this.InitializeMoveOrReplace(action, newItems, newItems, index, oldIndex);
        }

        public CollectionChangedEventArgs(ChangedAction action, Object newItem, Object oldItem, int index)
        {
            this._newStartingIndex = -1;
            this._oldStartingIndex = -1;
            if (action != ChangedAction.Replace)
            {
                throw new ArgumentException("Wrong Action For Ctor", "action");
            }
            this.InitializeMoveOrReplace(action, new object[] { newItem }, new object[] { oldItem }, index, index);
        }

        #endregion

        #region " Methods "
        private void InitializeAdd(ChangedAction action, IList newItems, int newStartingIndex)
        {
            this._notifyAction = action;
            this._newItemList = (newItems == null) ? null : ArrayList.ReadOnly(newItems);
            this._newStartingIndex = newStartingIndex;
        }

        private void InitializeAddOrRemove(ChangedAction action, IList changedItems, int startingIndex)
        {
            if (action == ChangedAction.Add)
            {
                this.InitializeAdd(action, changedItems, startingIndex);
            }
            else if (action == ChangedAction.Remove)
            {
                this.InitializeRemove(action, changedItems, startingIndex);
            }
        }

        private void InitializeMoveOrReplace(ChangedAction action, IList newItems, IList oldItems, int startingIndex, int oldStartingIndex)
        {

            this.InitializeAdd(action, newItems, startingIndex);

            this.InitializeRemove(action, oldItems, oldStartingIndex);
        }

        private void InitializeRemove(ChangedAction action, IList oldItems, int oldStartingIndex)
        {
            this._notifyAction = action;
            this._oldItemList = (oldItems == null) ? null : ArrayList.ReadOnly(oldItems);
            this._oldStartingIndex = oldStartingIndex;
        }

        #endregion

        #region " Properties "
        public ChangedAction Action
        {
            get
            {
                return this._notifyAction;
            }
        }

        public IList NewItems
        {
            get
            {
                return this._newItemList ?? new List<object>();
            }
        }

        public int NewStartingIndex
        {
            get
            {
                return this._newStartingIndex;
            }
        }

        public IList OldItems
        {
            get
            {
                return this._oldItemList ?? new List<object>();
            }
        }

        public int OldStartingIndex
        {
            get
            {
                return this._oldStartingIndex;
            }
        }
        #endregion
    }
    #endregion

    #region CollectionChangedEvent
    public delegate void PropertyChangedHandler(object sender, string propertyName);
    public delegate void CollectionChangedEventHandler(Object sender, CollectionChangedEventArgs changeArgs);
    public interface INotifyCollectionChanged
    {
        event CollectionChangedEventHandler CollectionChanged;
    }
    public interface IPropertyChanged
    {
        event PropertyChangedHandler PropertyChanged;
    }
    #endregion

    [Serializable]
    public class ObservableCollection<T> : Collection<T>, INotifyCollectionChanged, IPropertyChanged
    {
        [Serializable]
        sealed class SimpleMonitor : IDisposable
        {
            private int _busyCount;

            public void Enter()
            {
                _busyCount++;
            }

            public void Dispose()
            {
                _busyCount--;
            }

            public bool Busy
            {
                get { return _busyCount > 0; }
            }
        }

        private SimpleMonitor _monitor = new SimpleMonitor();

        public ObservableCollection()
        {

        }

        public ObservableCollection(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            foreach (var item in collection)
                Add(item);
        }

        public ObservableCollection(List<T> list) : base(list != null ? new List<T>(list) : null)
        {
        }

        [field: NonSerialized]
        public virtual event CollectionChangedEventHandler CollectionChanged;
        [field: NonSerialized]
        public virtual event PropertyChangedHandler PropertyChanged;

        event PropertyChangedHandler IPropertyChanged.PropertyChanged
        {
            add { PropertyChanged += value; }
            remove { PropertyChanged -= value; }
        }

        protected IDisposable BlockReentrancy()
        {
            _monitor.Enter();
            return _monitor;
        }

        protected void CheckReentrancy()
        {
            CollectionChangedEventHandler eh = CollectionChanged;

            // Only have a problem if we have more than one event listener.
            if (_monitor.Busy && eh != null && eh.GetInvocationList().Length > 1)
                throw new InvalidOperationException("Cannot modify the collection while reentrancy is blocked.");
        }

        protected override void ClearItems()
        {
            CheckReentrancy();
            OnCollectionChanged(this, new CollectionChangedEventArgs(ChangedAction.Reset));
            OnPropertyChanged("Count");
            OnPropertyChanged("Item[]");
            base.ClearItems();
        }

        protected override void InsertItem(int index, T item)
        {
            CheckReentrancy();

            base.InsertItem(index, item);

            OnCollectionChanged(this, new CollectionChangedEventArgs(ChangedAction.Add, item, index));
            OnPropertyChanged("Count");
            OnPropertyChanged("Item[]");
        }

        public void Move(int oldIndex, int newIndex)
        {
            MoveItem(oldIndex, newIndex);
        }

        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            CheckReentrancy();

            T item = this[oldIndex];
            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, item);

            OnCollectionChanged(this, new CollectionChangedEventArgs(ChangedAction.Move, item, newIndex, oldIndex));
            OnPropertyChanged("Item[]");
        }

        protected virtual void OnCollectionChanged(object sender, CollectionChangedEventArgs e)
        {
            CollectionChangedEventHandler eh = CollectionChanged;

            if (eh != null)
            {
                // Make sure that the invocation is done before the collection changes,
                // Otherwise there's a chance of data corruption.
                using (BlockReentrancy())
                {
                    eh(this, e);
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedHandler eh = PropertyChanged;

            if (eh != null)
                eh(this, propertyName);
        }

        protected override void RemoveItem(int index)
        {
            CheckReentrancy();

            T item = this[index];

            base.RemoveItem(index);

            OnCollectionChanged(this, new CollectionChangedEventArgs(ChangedAction.Remove, item, index));
            OnPropertyChanged("Count");
            OnPropertyChanged("Item[]");
        }

        protected override void SetItem(int index, T item)
        {
            CheckReentrancy();

            T oldItem = this[index];
            base.SetItem(index, item);
            OnCollectionChanged(this, new CollectionChangedEventArgs(ChangedAction.Replace, item, oldItem, index));
            OnPropertyChanged("Item[]");
        }
    }
}