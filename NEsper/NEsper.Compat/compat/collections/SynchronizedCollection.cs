///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace com.espertech.esper.compat.collections
{
    public class SynchronizedCollection<T> : ICollection<T>
    {
        /// <summary>
        /// Underlying set.
        /// </summary>
        private readonly ICollection<T> _facadeCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizedCollection{T}"/> class.
        /// </summary>
        /// <param name="facadeCollection">The facadeCollection.</param>
        public SynchronizedCollection(ICollection<T> facadeCollection)
        {
            _facadeCollection = facadeCollection;
        }

        #region ICollection<T> Members

        /// <summary>
        /// Converts the set to an array.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public T[] ToArray()
        {
            return _facadeCollection.ToArray();
        }

        /// <summary>
        /// Adds all of the items in the source.
        /// </summary>
        /// <param name="source">The source.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddAll(IEnumerable<T> source)
        {
            _facadeCollection.AddAll(source);
        }

        /// <summary>
        /// Returns the first item in the set
        /// </summary>
        /// <returns></returns>
        public T First
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return _facadeCollection.First(); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return _facadeCollection.IsEmpty(); }
        }

        /// <summary>
        /// Removes all items.
        /// </summary>
        /// <param name="items"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RemoveAll(IEnumerable<T> items)
        {
            _facadeCollection.RemoveAll(items);
        }

        #endregion

        #region ICollection<T> Members

        ///<summary>
        ///Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///</summary>
        ///
        ///<param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Add(T item)
        {
            _facadeCollection.Add(item);
        }

        ///<summary>
        ///Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///</summary>
        ///
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Clear()
        {
            _facadeCollection.Clear();
        }

        ///<summary>
        ///Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
        ///</summary>
        ///
        ///<returns>
        ///true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
        ///</returns>
        ///
        ///<param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Contains(T item)
        {
            return _facadeCollection.Contains(item);
        }

        ///<summary>
        ///Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        ///</summary>
        ///
        ///<param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        ///<param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        ///<exception cref="T:System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        ///<exception cref="T:System.ArgumentNullException">array is null.</exception>
        ///<exception cref="T:System.ArgumentException">array is multidimensional.-or-arrayIndex is equal to or greater than the length of array.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"></see> is greater than the available space from arrayIndex to the end of the destination array.-or-Type T cannot be cast automatically to the type of the destination array.</exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void CopyTo(T[] array, int arrayIndex)
        {
            _facadeCollection.CopyTo(array, arrayIndex);
        }

        ///<summary>
        ///Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///</summary>
        ///
        ///<returns>
        ///true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///</returns>
        ///
        ///<param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Remove(T item)
        {
            return _facadeCollection.Remove(item);
        }

        ///<summary>
        ///Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///</summary>
        ///
        ///<returns>
        ///The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///</returns>
        ///
        public int Count
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return _facadeCollection.Count; }
        }

        ///<summary>
        ///Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
        ///</summary>
        ///
        ///<returns>
        ///true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.
        ///</returns>
        ///
        public bool IsReadOnly
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return _facadeCollection.IsReadOnly; }
        }

        #endregion

        #region IEnumerable<T> Members

        ///<summary>
        ///Returns an enumerator that iterates through the collection.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        ///</returns>
        ///<filterpriority>1</filterpriority>
        [MethodImpl(MethodImplOptions.Synchronized)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _facadeCollection.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        ///<summary>
        ///Returns an enumerator that iterates through a collection.
        ///</summary>
        ///
        ///<returns>
        ///An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable<T>) this).GetEnumerator();
        }

        #endregion
    }
}
