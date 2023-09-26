///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.collection
{
    public class LRUCache<TK, TV> : IDictionary<TK, TV>
        where TK : class
    {
        private readonly int _cacheSize;
        private readonly IDictionary<TK, LinkedListNode<KeyValuePair<TK, TV>>> _entries;
        private readonly LinkedList<KeyValuePair<TK, TV>> _useOrderEntries;

        public LRUCache(int cacheSize)
        {
            _cacheSize = cacheSize;
            _useOrderEntries = new LinkedList<KeyValuePair<TK, TV>>();
            _entries = new HashMap<TK, LinkedListNode<KeyValuePair<TK, TV>>>();
        }

        public int Count => _entries.Count;
        public bool IsReadOnly => false;
        public ICollection<TK> Keys => _entries.Keys;

        public ICollection<TV> Values => new TransformCollection<KeyValuePair<TK, TV>, TV>(
            _useOrderEntries,
            v => throw new NotSupportedException(),
            kv => kv.Value);

        public void Add(KeyValuePair<TK, TV> item)
        {
            // first determine if the key already exists in the collection
            if (_entries.TryGetValue(item.Key, out var entry)) {
                var node = entry.Value;
                _useOrderEntries.Remove(node);
                _entries[item.Key] = _useOrderEntries.AddLast(item);
            }
            else {
                _entries[item.Key] = _useOrderEntries.AddLast(item);
            }

            TrimToCacheLimit();
        }

        public void Add(
            TK key,
            TV value)
        {
            Add(new KeyValuePair<TK, TV>(key, value));
        }

        public void Clear()
        {
            _useOrderEntries.Clear();
            _entries.Clear();
        }

        public bool Remove(KeyValuePair<TK, TV> item)
        {
            if (_entries.TryGetValue(item.Key, out var entry) && Equals(item.Value, entry.Value.Value)) {
                var node = entry.Value;
                _useOrderEntries.Remove(node);
                _entries.Remove(item.Key);
                return true;
            }

            return false;
        }

        public bool Remove(TK key)
        {
            if (_entries.TryGetValue(key, out var entry)) {
                var node = entry.Value;
                _useOrderEntries.Remove(node);
                _entries.Remove(key);
                return true;
            }

            return false;
        }

        public bool Contains(KeyValuePair<TK, TV> item)
        {
            if (_entries.TryGetValue(item.Key, out var entry) &&
                Equals(item.Value, entry.Value.Value)) {
                TouchEntry(entry);
            }

            return false;
        }

        public bool ContainsKey(TK key)
        {
            return _entries.ContainsKey(key);
        }

        public bool TryGetValue(
            TK key,
            out TV value)
        {
            if (_entries.TryGetValue(key, out var node)) {
                value = node.Value.Value;
                if (node != _useOrderEntries.Last) {
                    _useOrderEntries.Remove(node);
                    _useOrderEntries.AddLast(node);
                }

                return true;
            }

            value = default;
            return false;
        }

        public TV this[TK key] {
            get {
                if (TryGetValue(key, out var value)) {
                    return value;
                }

                throw new KeyNotFoundException();
            }
            set => Add(new KeyValuePair<TK, TV>(key, value));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
        {
            foreach (var keyValuePair in _useOrderEntries) {
                yield return keyValuePair;
            }
        }

        public void CopyTo(
            KeyValuePair<TK, TV>[] array,
            int arrayIndex)
        {
            _useOrderEntries.CopyTo(array, arrayIndex);
        }

        private void TouchEntry(LinkedListNode<KeyValuePair<TK, TV>> entry)
        {
            if (entry != _useOrderEntries.Last) {
                _useOrderEntries.Remove(entry);
                _useOrderEntries.AddLast(entry);
            }
        }

        private void TrimToCacheLimit()
        {
            while (_entries.Count > _cacheSize) {
                var leastRecentNode = _useOrderEntries.First;
                _useOrderEntries.Remove(leastRecentNode);
                _entries.Remove(leastRecentNode.Value.Key);
            }
        }
    }
} // end of namespace