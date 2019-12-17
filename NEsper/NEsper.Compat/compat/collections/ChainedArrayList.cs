///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

namespace com.espertech.esper.compat.collections
{
    public sealed class ChainedArrayList<T> : ICollection<T>
    {
        private int _count;
        private readonly int _chainLength;
        private Entry _chainHead;
        private Entry _chainTail;

        public ChainedArrayList(int chainLength)
        {
            _chainLength = chainLength;
            _chainHead = _chainTail = new Entry
            {
                Index = 0,
                Value = new T[_chainLength],
                Next = null
            };
        }

        public ChainedArrayList(IEnumerable<T> source, int chainLength)
        {
            _chainLength = chainLength;
            _chainHead = _chainTail = new Entry
            {
                Index = 0,
                Value = new T[_chainLength],
                Next = null
            };

            AddRange(source);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
#if true
            return new EnumeratorImpl(_chainHead, 0);
#else
            for (var curr = _chainHead; curr != null; curr = curr.Next)
            {
                for (int ii = 0; ii < curr.Index; ii++)
                {
                    yield return curr.ConstantValue[ii];
                }
            }
#endif
        }

        public void ForEach(Action<T> action)
        {
            for (var curr = _chainHead; curr != null; curr = curr.Next)
            {
                for (int ii = 0; ii < curr.Index; ii++)
                {
                    action.Invoke(curr.Value[ii]);
                }
            }
        }

        private void Expand()
        {
            var curr = new Entry
            {
                Index = 0,
                Value = new T[_chainLength],
                Next = null
            };

            _chainTail.Next = curr;
            _chainTail = curr;
        }

        public void Add(T item)
        {
            _count++;
            _chainTail.Value[_chainTail.Index++] = item;
            if (_chainTail.Index == _chainLength)
            {
                Expand();
            }
        }

        private void AddRange(IEnumerable<T> source)
        {
            var e = source.GetEnumerator();
            while (e.MoveNext())
            {
                Add(e.Current);
            }
        }

        public void Clear()
        {
            _count = 0;
            _chainHead = _chainTail = new Entry
            {
                Index = 0,
                Value = new T[_chainLength],
                Next = null
            };
        }

        public bool Contains(T item)
        {
            for (var curr = _chainHead; curr != null; curr = curr.Next)
            {
                for (int ii = 0; ii < curr.Index; ii++)
                {
                    if (Equals(item, curr.Value[ii]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (var curr = _chainHead; curr != null; curr = curr.Next)
            {
                for (int ii = 0; ii < curr.Index; ii++)
                {
                    array[arrayIndex++] = curr.Value[ii];
                }
            }
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public int Count => _count;

        public bool IsReadOnly => true;

        internal sealed class Entry
        {
            internal Entry Next;
            internal int Index;
            internal T[] Value;
        }

        internal sealed class EnumeratorImpl : IEnumerator<T>
        {
            private bool _hasNext;
            private Entry _entry;
            private int _index;
            private T _value;

            public EnumeratorImpl(Entry entry, int index)
            {
                _entry = entry;
                _index = index;
                _value = default(T);

                if (!(_hasNext = (_index < _entry.Index)))
                {
                    _index = 0;
                    _entry = _entry.Next; // probably null
                }
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_hasNext)
                {
                    // ldarg.0
                    // ldfld _indx
                    // ldarg.0
                    // stfld _value
                    _value = _entry.Value[_index];
                    if (++_index >= _entry.Index)
                    {
                        _index = 0;
                        _entry = _entry.Next;
                        _hasNext = _entry != null;
                    }

                    return true;
                }

                return false;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            object IEnumerator.Current => Current;

            public T Current => _value;
        }
    }
}