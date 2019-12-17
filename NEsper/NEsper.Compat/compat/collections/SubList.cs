using System;
using System.Collections;
using System.Collections.Generic;

namespace com.espertech.esper.compat.collections
{
    public class SubList<T> : IList<T>
    {
        private readonly IList<T> _parent;
        private readonly int _fromIndex;
        private int _toIndex;
        private int _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubList{T}"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="fromIndex">From index.</param>
        /// <param name="toIndex">To index.</param>
        public SubList(IList<T> parent, int fromIndex, int toIndex)
        {
            _parent = parent;
            _fromIndex = fromIndex;
            _toIndex = toIndex;
            _count = _toIndex - _fromIndex;
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException">
        /// </exception>
        public T this[int index]
        {
            get
            {
                BoundsCheck(index);
                return _parent[_fromIndex + index];
            }
            set
            {
                BoundsCheck(index);
                _parent[_fromIndex + index] = value;
            }
        }

        /// <summary>
        /// Checks the index to ensure it is within the bounds.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <exception cref="System.ArgumentException">
        /// index exceeds bounds
        /// or
        /// index exceeds bounds
        /// </exception>
        private void BoundsCheck(int index)
        {
            if (index < 0)
                throw new ArgumentException("index exceeds bounds");
            if (index >= _count)
                throw new ArgumentException("index exceeds bounds");
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        public void Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        public bool Contains(T item)
        {
            for (int ii = _fromIndex; ii < _toIndex; ii++)
            {
                if (Equals(_parent[ii], item))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Copies to list to the array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            var arrayLen = array.Length;
            if (arrayLen > _count)
                arrayLen = _count;

            for (int ii = 0, nn = _fromIndex; ii < arrayLen; ii++, nn++)
            {
                array[ii] = _parent[nn];
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <returns>
        /// The index of <paramref name="item" /> if found in the list; otherwise, -1.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int IndexOf(T item)
        {
            for (int ii = _fromIndex; ii < _toIndex; ii++)
            {
                if (Equals(_parent[ii], item))
                {
                    return ii - _fromIndex;
                }
            }

            return -1;
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public void Insert(int index, T item)
        {
            BoundsCheck(index);
            _parent.Insert(_fromIndex + index, item);
            _toIndex++;
            _count++;
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public void RemoveAt(int index)
        {
            BoundsCheck(index);
            _parent.RemoveAt(_fromIndex + index);
            _toIndex--;
            _count--;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerator<T> GetEnumerator()
        {
            for (int ii = _fromIndex; ii < _toIndex; ii++)
            {
                yield return _parent[ii];
            }
        }
    }
}
