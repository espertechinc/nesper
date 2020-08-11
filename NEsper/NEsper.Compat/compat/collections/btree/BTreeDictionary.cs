using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.compat.collections.bound;

namespace com.espertech.esper.compat.collections.btree
{
    public class BTreeDictionary<TK, TV> : IOrderedDictionary<TK, TV>
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        public BTreeDictionary()
        {
            Underlying = new BTree<TK, KeyValuePair<TK, TV>>(_ => _.Key, Comparer<TK>.Default);
            Keys = new BTreeDictionaryKeys<TK, TV>(Underlying);
            Values = new BTreeDictionaryValues<TK, TV>(Underlying);
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="comparer"></param>
        public BTreeDictionary(IComparer<TK> comparer)
        {
            Underlying = new BTree<TK, KeyValuePair<TK, TV>>(_ => _.Key, comparer);
            Keys = new BTreeDictionaryKeys<TK, TV>(Underlying);
            Values = new BTreeDictionaryValues<TK, TV>(Underlying);
        }

        /// <summary>
        ///     Returns the underlying btree.
        /// </summary>
        public BTree<TK, KeyValuePair<TK, TV>> Underlying { get; }

        /// <summary>
        ///     Returns the key comparer.
        /// </summary>
        public IComparer<TK> KeyComparer => Underlying.KeyComparer;

        /// <summary>
        ///     Gets the height of the tree.
        /// </summary>
        /// <returns>
        ///     The height of the tree.
        /// </returns>
        public int Height => Underlying.Height;

        /// <summary>
        ///     Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>
        ///     The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        public int Count => Underlying.Count;

        /// <summary>
        ///     Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <returns>
        ///     true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly => false;

        /// <summary>
        ///     Returns the first key-value pair in the dictionary.  If the dictionary
        ///     is empty, this method throws an InvalidOperationException.
        /// </summary>
        public KeyValuePair<TK, TV> FirstEntry {
            get {
                if (Underlying.Count == 0) {
                    throw new InvalidOperationException();
                }

                return Underlying
                    .Begin()
                    .Value;
            }
        }

        /// <summary>
        ///     Returns the last key-value pair in the dictionary.  If the dictionary
        ///     is empty, this method throws an InvalidOperationException.
        /// </summary>
        public KeyValuePair<TK, TV> LastEntry {
            get {
                if (Underlying.Count == 0) {
                    throw new InvalidOperationException();
                }

                return Underlying
                    .End()
                    .MovePrevious()
                    .Value;
            }
        }

        /// <summary>
        /// Returns a readonly ordered dictionary that includes everything before the value.
        /// Whether the value is included in the range depends on whether the isInclusive
        /// flag is set.
        /// </summary>
        /// <param name="value">The end value.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        public IOrderedDictionary<TK, TV> Head(
            TK value,
            bool isInclusive = false)
        {
            return new BTreeDictionaryView<TK, TV>(
                this,
                new BoundRange<TK>(null, new Bound<TK>(value, isInclusive), KeyComparer));
        }

        /// <summary>
        /// Returns a readonly ordered dictionary that includes everything after the value.
        /// Whether the value is included in the range depends on whether the isInclusive
        /// flag is set.
        /// </summary>
        /// <param name="value">The end value.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        public IOrderedDictionary<TK, TV> Tail(
            TK value,
            bool isInclusive = true)
        {
            return new BTreeDictionaryView<TK, TV>(
                this,
                new BoundRange<TK>(new Bound<TK>(value, isInclusive), null, KeyComparer));
        }

        /// <summary>
        /// Returns a readonly ordered dictionary that includes everything between the
        /// two provided values.  Whether each value is included in the range depends
        /// on whether the isInclusive flag is set.
        /// </summary>
        /// <returns></returns>
        public IOrderedDictionary<TK, TV> Between(
            TK startValue,
            bool isStartInclusive,
            TK endValue,
            bool isEndInclusive)
        {
            return new BTreeDictionaryView<TK, TV>(
                this,
                new BoundRange<TK>(
                    new Bound<TK>(startValue, isStartInclusive),
                    new Bound<TK>(endValue, isEndInclusive),
                    KeyComparer));
        }

        /// <summary>
        ///     Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        ///     true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        public bool Contains(KeyValuePair<TK, TV> item)
        {
            var cursor = Underlying.FindUnique(item.Key, Underlying.RootCursor);
            if (cursor.Node != null) {
                return Equals(item.Key, cursor.Key) &&
                       Equals(item.Value, cursor.Value.Value);
            }

            return false;
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
            var cursor = Underlying.FindUnique(key, Underlying.RootCursor);
            if (cursor.Node != null) {
                return Equals(key, cursor.Key);
            }

            return false;
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the least key greater than or equal to the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<TK, TV>? GreaterThanOrEqualTo(TK key)
        {
            var cursor = Underlying.GreaterThanOrEqual(key, Underlying.RootCursor);
            if (cursor.IsEnd) {
                return default;
            }

            return cursor.Value;
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
            var cursor = Underlying.GreaterThanOrEqual(key, Underlying.RootCursor);
            if (cursor.IsEnd) {
                valuePair = default;
                return false;
            }

            valuePair = cursor.Value;
            return true;
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the greatest key less than or equal to the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<TK, TV>? LessThanOrEqualTo(TK key)
        {
            var cursor = Underlying.LessThanOrEqual(key, Underlying.RootCursor);
            if (cursor.IsEnd) {
                return default;
            }

            return cursor.Value;
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
            var cursor = Underlying.LessThanOrEqual(key, Underlying.RootCursor);
            if (cursor.IsEnd) {
                valuePair = default;
                return false;
            }

            valuePair = cursor.Value;
            return true;
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the least key strictly greater than the given key.
        ///     Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<TK, TV>? GreaterThan(TK key)
        {
            var cursor = Underlying.GreaterThan(key, Underlying.RootCursor);
            if (cursor.IsEnd) {
                return default;
            }

            return cursor.Value;
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
            var cursor = Underlying.GreaterThan(key, Underlying.RootCursor);
            if (cursor.IsEnd) {
                valuePair = default;
                return false;
            }

            valuePair = cursor.Value;
            return true;
        }

        /// <summary>
        ///     Finds a key-value mapping associated with the greatest key strictly less than the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<TK, TV>? LessThan(TK key)
        {
            var cursor = Underlying.LessThan(key, Underlying.RootCursor);
            if (cursor.IsEnd) {
                return default;
            }

            return cursor.Value;
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
            var cursor = Underlying.LessThan(key, Underlying.RootCursor);
            if (cursor.IsEnd) {
                valuePair = default;
                return false;
            }

            valuePair = cursor.Value;
            return true;
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
            var kvpair = new KeyValuePair<TK, TV>(key, value);
            var result = Underlying.InsertUnique(kvpair);
            if (!result.Succeeded) {
                throw new ArgumentException("An element with the same key already exists");
            }
        }

        /// <summary>
        ///     Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        public void Add(KeyValuePair<TK, TV> item)
        {
            var result = Underlying.InsertUnique(item);
            if (!result.Succeeded) {
                throw new ArgumentException("An element with the same key already exists");
            }
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
            var cursor = Underlying.FindUnique(item.Key, Underlying.RootCursor);
            if (cursor.Node != null) {
                if (Equals(cursor.Value.Value, item.Value)) {
                    Underlying.Erase(cursor);
                    return true;
                }
            }

            return false;
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
            //Console.WriteLine("removing {0}", key);
            return Underlying.TryEraseUnique(key, out var value);
        }

        /// <summary>
        ///     Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">
        ///     The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </exception>
        public void Clear()
        {
            Underlying.Clear();
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
            var cursor = Underlying.FindUnique(key, Underlying.RootCursor);
            if (cursor.Node != null) {
                value = cursor.Value.Value;
                return true;
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
                var cursor = Underlying.FindUnique(key, Underlying.RootCursor);
                if (cursor.Node != null) {
                    return cursor.Value.Value;
                }

                throw new KeyNotFoundException();
            }
            set {
                var kvpair = new KeyValuePair<TK, TV>(key, value);
                var result = Underlying.InsertUnique(kvpair);
                if (!result.Succeeded) {
                    result.Cursor.Value = kvpair;
                }
            }
        }

        /// <summary>
        ///     Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the
        ///     <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the object that implements
        ///     <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        public ICollection<TK> Keys { get; }

        /// <summary>
        ///     Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the
        ///     <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the object that implements
        ///     <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        public ICollection<TV> Values { get; }

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

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
        {
            return Underlying
                .Begin()
                .ToEnumerator();
        }
        
        /// <summary>
        ///     Returns an enumerator starting at a key-value with the greatest key less than
        ///     or equal to the given key.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<TK, TV>> EnumerateBetween(
            Bound<TK> start,
            Bound<TK> end)
        {
            var isEnd = BTreeDictionaryExtensions
                .GetEndPredicate(end, KeyComparer);
            var cursor = BTreeDictionaryExtensions
                .GetStartCursor(Underlying, start);
            while (cursor.IsNotEnd && !isEnd(cursor.Key)) {
                yield return cursor.Value;
                cursor.MoveNext();
            }
        }

        /// <summary>
        ///     Returns an enumerator starting at a key-value mapping associated with the least key greater than
        ///     or equal to the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<TK, TV>> EnumerateGreaterThanOrEqual(TK key)
        {
            return Underlying
                .GreaterThanOrEqual(key, Underlying.RootCursor)
                .ToEnumerable();
        }

        /// <summary>
        ///     Returns an enumerator starting at a key-value with the greatest key less than
        ///     or equal to the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<TK, TV>> EnumerateLessThanOrEqual(TK key)
        {
            return Underlying
                .LessThanOrEqual(key, Underlying.RootCursor)
                .ToEnumerable();
        }

        /// <summary>
        ///     Returns an enumerator starting at a key-value mapping associated with the least key strictly
        ///     greater than the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<TK, TV>> EnumerateGreater(TK key)
        {
            return Underlying
                .GreaterThan(key, Underlying.RootCursor)
                .ToEnumerable();
        }

        /// <summary>
        ///     Returns an enumerator starting at a key-value mapping associated with the greatest key
        ///     strictly less than the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<TK, TV>> EnumerateLessThan(TK key)
        {
            return Underlying
                .LessThan(key, Underlying.RootCursor)
                .ToEnumerable();
        }
    }
}