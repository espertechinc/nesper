///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.compat.collections
{
    public class CopyOnWriteList<T> : IList<T>
    {
        private T[] _arrayList;
        private readonly ILockable _writerLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyOnWriteList&lt;T&gt;"/> class.
        /// </summary>
        public CopyOnWriteList()
        {
            _arrayList = new T[0];
            _writerLock = new MonitorSlimLock(60000);
        }

        /// <summary>
        /// Gets the write lock.
        /// </summary>
        /// <value>The write lock.</value>
        public ILockable WriteLock => _writerLock;

        /// <summary>
        /// Converts the list to an array.
        /// </summary>
        /// <returns></returns>
        public T[] ToArray()
        {
            return _arrayList;
        }

        /// <summary>
        /// Iterates over each item in the list executing the specified
        /// action.
        /// </summary>
        /// <param name="action">The action.</param>
        public void ForEach(Action<T> action)
        {
            var list = _arrayList;
            var length = list.Length;
            for (int ii = 0; ii < length; ii++ )
            {
                action.Invoke(list[ii]);
            }
        }

        #region ICollection<T> Members

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        public virtual void Add(T item)
        {
            using (_writerLock.Acquire())
            {
                List<T> tempList = new List<T>(_arrayList);
                tempList.Add(item);
                _arrayList = tempList.ToArray();
            }
        }

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        public virtual void AddRange(IEnumerable<T> itemList)
        {
            using (_writerLock.Acquire())
            {
                List<T> tempList = new List<T>(_arrayList);
                tempList.AddRange(itemList);
                _arrayList = tempList.ToArray();
            }
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>
        public virtual void Clear()
        {
            using (_writerLock.Acquire())
            {
                _arrayList = new T[0];
            }
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
        /// </returns>
        public virtual bool Contains(T item)
        {
            return _arrayList.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        /// <exception cref="T:System.ArgumentNullException">array is null.</exception>
        /// <exception cref="T:System.ArgumentException">array is multidimensional.-or-arrayIndex is equal to or greater than the length of array.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"></see> is greater than the available space from arrayIndex to the end of the destination array.-or-Type T cannot be cast automatically to the type of the destination array.</exception>
        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            _arrayList.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
        public virtual int Count => _arrayList.Length;

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
        public virtual bool IsReadOnly => false;

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        public bool Remove(T item)
        {
            bool result;

            using (_writerLock.Acquire())
            {
                List<T> tempList = new List<T>(_arrayList);
                result = tempList.Remove(item);
                if (result)
                {
                    _arrayList = tempList.ToArray();
                }
            }

            return result;
        }

        /// <summary>
        /// Removes all items.
        /// </summary>
        /// <param name="items"></param>
        public void RemoveAll(IEnumerable<T> items)
        {
            using (_writerLock.Acquire())
            {
                List<T> tempList = new List<T>(_arrayList);
                foreach (T item in items)
                {
                    tempList.Remove(item);
                }
                _arrayList = tempList.ToArray();
            }
        }

        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            var list = _arrayList;
            var length = list.Length;
            for (int ii = 0; ii < length; ii++)
            {
                yield return list[ii];
            }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _arrayList.GetEnumerator();
        }

        #endregion

        #region IList<T> Members

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"></see>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        /// <returns>
        /// The index of item if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(T item)
        {
            return Array.IndexOf(_arrayList, item);
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"></see> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
        public void Insert(int index, T item)
        {
            using (_writerLock.Acquire())
            {
                List<T> tempList = new List<T>(_arrayList);
                tempList.Insert(index, item);
                _arrayList = tempList.ToArray();
            }
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1"></see> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
        public void RemoveAt(int index)
        {
            using (_writerLock.Acquire())
            {
                List<T> tempList = new List<T>(_arrayList);
                tempList.RemoveAt(index);
                _arrayList = tempList.ToArray();
            }
        }

        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <value></value>
        public T this[int index]
        {
            get => _arrayList[index];
            set
            {
                // Should this be using copy-on-write semantics or
                // is the set instruction essentially atomic?  Let's
                // not take any chances.
                
                using (_writerLock.Acquire())
                {
                    List<T> tempList = new List<T>(_arrayList);
                    tempList[index] = value;
                    _arrayList = tempList.ToArray();
                }
            }
        }

        #endregion
    }
}
