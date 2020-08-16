using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.compat.collections.bound;

namespace com.espertech.esper.compat.collections.btree
{
    public class BTreeDictionaryView<TK, TV> : IOrderedDictionary<TK, TV>
    {
        private readonly BTreeDictionary<TK, TV> _parent;
        private readonly BoundRange<TK> _range;
        private readonly BTreeDictionaryKeys<TK, TV> _keys;
        private readonly BTreeDictionaryValues<TK, TV> _values;

        /// <summary>
        ///     Constructor.
        /// </summary>
        public BTreeDictionaryView(
            BTreeDictionary<TK, TV> parent,
            BoundRange<TK> range)
        {
            _parent = parent;
            _range = range;
            _keys = new BTreeDictionaryKeys<TK, TV>(_parent.Underlying, _range);
            _values = new BTreeDictionaryValues<TK, TV>(_parent.Underlying, _range);
        }

        /// <summary>
        ///     Returns the key comparer.
        /// </summary>
        public IComparer<TK> KeyComparer => _parent.KeyComparer;

        /// <summary>
        ///     Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>
        ///     The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        public int Count => BTreeDictionaryExtensions.Count(_parent.Underlying, _range);

        /// <summary>
        ///     Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <returns>
        ///     true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly => true;

        /// <summary>
        ///     Returns a readonly ordered dictionary that includes everything before the value.
        ///     Whether the value is included in the range depends on whether the isInclusive
        ///     flag is set.
        /// </summary>
        /// <param name="value">The end value.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        public IOrderedDictionary<TK, TV> Head(
            TK value,
            bool isInclusive = false)
        {
            return new BTreeDictionaryView<TK, TV>(
                _parent,
                _range.Merge(
                    new BoundRange<TK>(
                        null,
                        new Bound<TK>(value, isInclusive),
                        KeyComparer)));
        }

        /// <summary>
        ///     Returns a readonly ordered dictionary that includes everything after the value.
        ///     Whether the value is included in the range depends on whether the isInclusive
        ///     flag is set.
        /// </summary>
        /// <param name="value">The end value.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        public IOrderedDictionary<TK, TV> Tail(
            TK value,
            bool isInclusive = true)
        {
            return new BTreeDictionaryView<TK, TV>(
                _parent,
                _range.Merge(
                    new BoundRange<TK>(
                        new Bound<TK>(value, isInclusive),
                        null,
                        KeyComparer)));
        }

        /// <summary>
        ///     Returns a readonly ordered dictionary that includes everything between the
        ///     two provided values.  Whether each value is included in the range depends
        ///     on whether the isInclusive flag is set.
        /// </summary>
        /// <returns></returns>
        public IOrderedDictionary<TK, TV> Between(
            TK startValue,
            bool isStartInclusive,
            TK endValue,
            bool isEndInclusive)
        {
            return new BTreeDictionaryView<TK, TV>(
                _parent,
                _range.Merge(
                    new BoundRange<TK>(
                        new Bound<TK>(startValue, isStartInclusive),
                        new Bound<TK>(endValue, isEndInclusive),
                        KeyComparer)));
        }

        /// <summary>
        ///     Returns the first key-value pair in the dictionary.  If the dictionary
        ///     is empty, this method throws an InvalidOperationException.
        /// </summary>
        public KeyValuePair<TK, TV> FirstEntry => BTreeDictionaryExtensions
            .FirstKeyValuePair(_parent.Underlying, _range);

        /// <summary>
        ///     Returns the last key-value pair in the dictionary.  If the dictionary
        ///     is empty, this method throws an InvalidOperationException.
        /// </summary>
        public KeyValuePair<TK, TV> LastEntry => BTreeDictionaryExtensions
            .LastKeyValuePair(_parent.Underlying, _range);

        /// <summary>
        ///     Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        ///     true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        public bool Contains(KeyValuePair<TK, TV> item)
        {
            return _range.IsWithin(item.Key) && _parent.Contains(item);
        }

        /// <summary>
        ///     Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
        /// <returns>
        ///     true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="key" /> is null.
        /// </exception>
        public bool ContainsKey(TK key)
        {
            return _range.IsWithin(key) && _parent.ContainsKey(key);
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the least key greater than or equal to the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<TK, TV>? GreaterThanOrEqualTo(TK key)
        {
            var result = _parent.GreaterThanOrEqualTo(key);
            if (result.HasValue && _range.IsWithin(result.Value.Key)) {
                return result;
            }

            return default;
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the least key greater than or equal to the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valuePair"></param>
        /// <returns></returns>
        public bool TryGreaterThanOrEqualTo(
            TK key,
            out KeyValuePair<TK, TV> valuePair)
        {
            if (_parent.TryGreaterThanOrEqualTo(key, out var result)) {
                if (_range.IsWithin(result.Key)) {
                    valuePair = result;
                    return true;
                }
            }

            valuePair = default;
            return false;
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the greatest key less than or equal to the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<TK, TV>? LessThanOrEqualTo(TK key)
        {
            var result = _parent.LessThanOrEqualTo(key);
            if (result.HasValue && _range.IsWithin(result.Value.Key)) {
                return result;
            }

            return default;
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the greatest key less than or equal to the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valuePair"></param>
        /// <returns></returns>
        public bool TryLessThanOrEqualTo(
            TK key,
            out KeyValuePair<TK, TV> valuePair)
        {
            if (_parent.TryLessThanOrEqualTo(key, out var result)) {
                if (_range.IsWithin(result.Key)) {
                    valuePair = result;
                    return true;
                }
            }

            valuePair = default;
            return false;
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the least key strictly greater than the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<TK, TV>? GreaterThan(TK key)
        {
            var result = _parent.GreaterThan(key);
            if (result.HasValue && _range.IsWithin(result.Value.Key)) {
                return result;
            }

            return default;
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the least key strictly greater than the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valuePair"></param>
        /// <returns></returns>
        public bool TryGreaterThan(
            TK key,
            out KeyValuePair<TK, TV> valuePair)
        {
            if (_parent.TryGreaterThan(key, out var result)) {
                if (_range.IsWithin(result.Key)) {
                    valuePair = result;
                    return true;
                }
            }

            valuePair = default;
            return false;
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the greatest key strictly less than the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<TK, TV>? LessThan(TK key)
        {
            var result = _parent.LessThan(key);
            if (result.HasValue && _range.IsWithin(result.Value.Key)) {
                return result;
            }

            return default;
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the greatest key strictly less than the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valuePair"></param>
        /// <returns></returns>
        public bool TryLessThan(
            TK key,
            out KeyValuePair<TK, TV> valuePair)
        {
            if (_parent.TryLessThan(key, out var result)) {
                if (_range.IsWithin(result.Key)) {
                    valuePair = result;
                    return true;
                }
            }

            valuePair = default;
            return false;
        }

        /// <summary>
        ///     Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="key" /> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///     The <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.
        /// </exception>
        public void Add(
            TK key,
            TV value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        public void Add(KeyValuePair<TK, TV> item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        ///     true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This
        ///     method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        public bool Remove(KeyValuePair<TK, TV> item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        ///     true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the
        ///     original <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="key" /> is null.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.</exception>
        public bool Remove(TK key)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">
        ///     The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </exception>
        public void Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>Gets the value associated with the specified key.</summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">
        ///     When this method returns, the value associated with the specified key, if the key is found; otherwise,
        ///     the default value for the type of
        ///     the <paramref name="value" /> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an
        ///     element with the specified key; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="key" /> is null.
        /// </exception>
        public bool TryGetValue(
            TK key,
            out TV value)
        {
            if (_range.IsWithin(key)) {
                return _parent.TryGetValue(key, out value);
            }

            value = default;
            return false;
        }

        /// <summary>Gets or sets the element with the specified key.</summary>
        /// <param name="key">The key of the element to get or set.</param>
        /// <returns>The element with the specified key.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="key" /> is null.
        /// </exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key" /> is not found.</exception>
        /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.</exception>
        public TV this[TK key] {
            get {
                if (_range.IsWithin(key)) {
                    return _parent[key];
                }

                throw new KeyNotFoundException();
            }
            set => throw new NotSupportedException();
        }

        /// <summary>
        ///     Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the
        ///     <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the object that implements
        ///     <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        public ICollection<TK> Keys => _keys;

        /// <summary>
        /// Returns the keys as an ordered collection.
        /// </summary>
        public IOrderedCollection<TK> OrderedKeys => _keys;

        /// <summary>
        ///     Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the
        ///     <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the object that implements
        ///     <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        public ICollection<TV> Values => _values;

        /// <summary>
        ///     Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />,
        ///     starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from
        ///     <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="array" /> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="arrayIndex" /> is less than 0.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the
        ///     available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.
        /// </exception>
        public void CopyTo(
            KeyValuePair<TK, TV>[] array,
            int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
        {
            return BTreeDictionaryExtensions
                .Enumerate(_parent.Underlying, _range)
                .GetEnumerator();
        }
        
        /// <summary>
        /// Returns an inverted version of the dictionary.
        /// </summary>
        /// <returns></returns>
        public IOrderedDictionary<TK, TV> Invert()
        {
            var inverted = (BTreeDictionary<TK, TV>) _parent.Invert();
            return new BTreeDictionaryView<TK, TV>(inverted, _range);
        }
    }
}