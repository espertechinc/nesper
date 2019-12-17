///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

namespace com.espertech.esper.compat.collections
{
	/// <summary>
	/// Collection that wraps another collection + an item
	/// </summary>
	public class CollectionPlus<T> : ICollection<T>
	{
		private readonly ICollection<T> m_baseCollection ;
		private readonly T m_additionalItem;
		
        /// <summary>
        /// Constructs a new collection plus an item
        /// </summary>
        /// <param name="baseCollection"></param>
        /// <param name="item"></param>
		public CollectionPlus(ICollection<T> baseCollection, T item)
		{
			this.m_baseCollection = baseCollection;
			this.m_additionalItem = item;
		}

	    ///<summary>
	    ///Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
	    ///</summary>
	    ///
	    ///<returns>
	    ///The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
	    ///</returns>
	    ///
	    public int Count => m_baseCollection.Count + 1;

	    ///<summary>
	    ///Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
	    ///</summary>
	    ///
	    ///<returns>
	    ///true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.
	    ///</returns>
	    ///
	    public bool IsReadOnly => true;

	    ///<summary>
	    ///Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
	    ///</summary>
	    ///
	    ///<param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
	    ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
	    public void Add(T item)
		{
			throw new NotSupportedException();
		}

	    ///<summary>
	    ///Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
	    ///</summary>
	    ///
	    ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>
	    public void Clear()
		{
			throw new NotSupportedException();
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
	    public bool Contains(T item)
		{
			if ( Equals( m_additionalItem, item ) ) {
				return true ;
			}
			
			return m_baseCollection.Contains(item);
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
	    public void CopyTo(T[] array, int arrayIndex)
		{
			array[arrayIndex++] = m_additionalItem;
			m_baseCollection.CopyTo(array, arrayIndex);
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
	    public bool Remove(T item)
		{
			throw new NotSupportedException();
		}

	    ///<summary>
	    ///Returns an enumerator that iterates through the collection.
	    ///</summary>
	    ///
	    ///<returns>
	    ///A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
	    ///</returns>
	    ///<filterpriority>1</filterpriority>
	    public IEnumerator<T> GetEnumerator()
		{
			yield return m_additionalItem ;
			
			foreach( T item in m_baseCollection ) {
				yield return item;
			}
		}

	    ///<summary>
	    ///Returns an enumerator that iterates through a collection.
	    ///</summary>
	    ///
	    ///<returns>
	    ///An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
	    ///</returns>
	    ///<filterpriority>2</filterpriority>
	    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			yield return m_additionalItem ;
			
			foreach( T item in m_baseCollection ) {
				yield return item;
			}
		}
	}
}
