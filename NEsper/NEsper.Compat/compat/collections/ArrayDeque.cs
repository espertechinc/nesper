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
    [Serializable]
    public class ArrayDeque<T> : Deque<T>
    {
        private const int DEFAULT_INITIAL_CAPACITY = 256;

        private int _head;
        private int _tail;
        private T[] _array;

        public ArrayDeque() : this(DEFAULT_INITIAL_CAPACITY)
        {
        }

        public ArrayDeque(int capacity)
        {
            _head = 0;
            _tail = 0;
            _array = new T[capacity];
        }

        public ArrayDeque(ICollection<T> source)
        {
            var ncount = source.Count;
            var lcount = (int) Math.Log(ncount, 2);
            var mcount = 1 << lcount;
            if (mcount <= ncount)
                mcount <<= 1;

            _array = new T[mcount];
            source.CopyTo(_array, 0);
            _head = 0;
            _tail = ncount;
        }

        public T[] Array
        {
            get
            {
                var array = new T[Count];
                CopyTo(array, 0);
                return array;
            }
        }
            
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_tail == _head)
            {
            }
            else if (_tail > _head)
            {
                for (int ii = _head; ii < _tail; ii++)
                    yield return _array[ii];
            }
            else
            {
                for (int ii = _head; ii < _array.Length; ii++)
                    yield return _array[ii];
                for (int ii = 0; ii < _tail; ii++)
                    yield return _array[ii];
            }
        }

        public void Visit(Action<T> action)
        {
            if (_tail == _head)
            {
            }
            else if (_tail > _head)
            {
                for (int ii = _head; ii < _tail; ii++)
                    action.Invoke(_array[ii]);
            }
            else
            {
                for (int ii = _head; ii < _array.Length; ii++)
                    action.Invoke(_array[ii]);
                for (int ii = 0; ii < _tail; ii++)
                    action.Invoke(_array[ii]);
            }
        }

        private void DoubleCapacity()
        {
            int newLength = _array.Length << 1;
            if (newLength < 0)
                throw new IllegalStateException("ArrayDeque overflow");

            var narray = new T[newLength];

            if (_tail > _head)
            {
                System.Array.Copy(_array, _head, narray, 0, _tail - _head);
            }
            else
            {
                var nl = _head;
                var nr = _array.Length - _head;

                System.Array.Copy(_array, _head, narray, 0, nr);
                System.Array.Copy(_array, 0, narray, nr, nl);
            }

            _head = 0;
            _tail = _array.Length;
            _array = narray;
        }

        public void AddFirst(T item)
        {
            if (--_head < 0)
                _head = _array.Length - 1;
            if (_head == _tail)
                DoubleCapacity();
            _array[_head] = item;
        }

        public void AddLast(T item)
        {
            _array[_tail] = item;
            if (++_tail == _array.Length)
                _tail = 0;
            if (_head == _tail)
                DoubleCapacity();
        }

        public void Add(T item)
        {
            AddLast(item);
        }

        public T RemoveFirst()
        {
            if (_head == _tail)
                throw new NoSuchElementException();
            // preserve the value at the head
            var result = _array[_head];
            // clear the value in the array
            _array[_head] = default(T);
            // increment the head
            if (++_head == _array.Length)
                _head = 0;

            return result;
        }

        public T RemoveLast()
        {
            if (_tail == _head)
                throw new NoSuchElementException();
            if (--_tail < 0)
                _tail = _array.Length - 1;
            var result = _array[_tail];
            _array[_tail] = default(T);
            return result;
        }

        public TM RemoveInternal<TM>(ref int index, TM returnValue)
        {
            if (_tail > _head)
            {
                int tindex = index;
                if (tindex >= _tail)
                    throw new ArgumentOutOfRangeException();
                if (tindex == _head)
                {
                    _array[_head++] = default(T);
                    _head %= _array.Length;
                    index = _head;
                }
                else
                {
                    for (int ii = tindex + 1; ii < _tail; ii++)
                        _array[ii - 1] = _array[ii];

                    if (--_tail < 0)
                        _tail = _array.Length - 1;
                    _array[_tail] = default(T);
                }
            }
            else if (index > _array.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            else
            {
                int tindex = index;
                if (tindex == _head)
                {
                    _array[_head++] = default(T);
                    _head %= _array.Length;
                    index = _head;
                }
                else if (tindex > _head)
                {
                    for (int ii = tindex + 1 ; ii < _array.Length ; ii++)
                        _array[ii - 1] = _array[ii];
                    _array[_array.Length - 1] = _array[0];
                    for (int ii = 1 ; ii < _tail ; ii++)
                        _array[ii - 1] = _array[ii];

                    if (--_tail < 0)
                        _tail = _array.Length - 1;

                    _array[_tail] = default(T);
                }
                else
                {
                    for (int ii = 1 ; ii < _tail ; ii++)
                        _array[ii - 1] = _array[ii];

                    if (--_tail < 0)
                        _tail = _array.Length - 1;
                    _array[_tail] = default(T);
                }
            }

            return returnValue;
        }

        public void RemoveAt(int index)
        {
            index = (_head + index) % _array.Length;
            RemoveInternal(ref index, 0);
        }

        public void RemoveWhere(
            Func<T, Continuation, bool> handler,
            Action<T> onRemoveItem = null)
        {
            T value;

            var continuation = new Continuation(true);

            if (_tail > _head)
            {
                for (int ii = _head; continuation.Value && ii < _tail; ii++)
                {
                    if (handler.Invoke(value = _array[ii], continuation))
                    {
                        if (RemoveInternal(ref ii, true))
                            onRemoveItem?.Invoke(value);
                        --ii;
                    }
                }
            }
            else if (_tail != _head)
            {
                for (int ii = _head; continuation.Value && ii < _array.Length; ii++)
                {
                    if (handler.Invoke(value = _array[ii], continuation))
                    {
                        if (RemoveInternal(ref ii, true))
                            onRemoveItem?.Invoke(value);
                        --ii;
                    }
                }
                for (int ii = 0; continuation.Value && ii < _tail; ii++)
                {
                    if (handler.Invoke(value = _array[ii], continuation))
                    {
                        if (RemoveInternal(ref ii, true))
                            onRemoveItem?.Invoke(value);
                        --ii;
                    }
                }
            }
        }

        public void RemoveWhere(
            Func<T, bool> predicate,
            Action<T> onRemoveItem = null)
        {
            T value;

            if (_tail > _head)
            {
                for (int ii = _head; ii < _tail; ii++)
                {
                    if (predicate.Invoke(value = _array[ii]))
                    {
                        if (RemoveInternal(ref ii, true))
                            onRemoveItem?.Invoke(value);
                        --ii;
                    }
                }
            }
            else if (_tail != _head)
            {
                for (int ii = _head; ii < _array.Length; ii++)
                {
                    if (predicate.Invoke(value = _array[ii]))
                    {
                        if (RemoveInternal(ref ii, true))
                            onRemoveItem?.Invoke(value);
                        --ii;
                    }
                }
                for (int ii = 0; ii < _tail; ii++)
                {
                    if (predicate.Invoke(value = _array[ii]))
                    {
                        if (RemoveInternal(ref ii, true))
                            onRemoveItem?.Invoke(value);
                        --ii;
                    }
                }
            }
        }
        
        public bool Remove(T item)
        {
            if (_tail > _head)
            {
                for (int ii = _head; ii < _tail; ii++)
                    if (Equals(_array[ii], item))
                        return RemoveInternal(ref ii, true);
            }
            else
            {
                for (int ii = _head; ii < _array.Length; ii++)
                    if (Equals(_array[ii], item))
                        return RemoveInternal(ref ii, true);
                for (int ii = 0; ii < _tail; ii++)
                    if (Equals(_array[ii], item))
                        return RemoveInternal(ref ii, true);
            }

            return false;
        }

        public void Clear()
        {
            _head = 0;
            _tail = 0;
            _array.Fill(default(T));
        }

        public bool Contains(T item)
        {
            if (_tail > _head)
            {
                for (int ii = _head; ii < _tail; ii++)
                    if (Equals(_array[ii], item))
                        return true;
            }
            else
            {
                for (int ii = _head; ii < _array.Length; ii++)
                    if (Equals(_array[ii], item))
                        return true;
                for (int ii = 0; ii < _tail; ii++)
                    if (Equals(_array[ii], item))
                        return true;
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (_tail == _head)
            {
                    
            }
            else if (_tail > _head)
            {
                System.Array.Copy(_array, _head, array, 0, _tail - _head);
            }
            else
            {
                var nl = _head;
                var nr = _array.Length - _head;

                System.Array.Copy(_array, _head, array, 0, nr);
                System.Array.Copy(_array, 0, array, nr, nl);
            }
        }

        public T this[int index]
        {
            get
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException();
                if (_tail > _head)
                    return _array[_head + index];
                if (index >= _tail + _array.Length - _head)
                    throw new ArgumentOutOfRangeException();

                var offset = _head + index;
                if (offset < _array.Length)
                    return _array[offset];

                return _array[offset%_array.Length];
            }

            set
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException();
                if (_tail > _head)
                {
                    _array[_head + index] = value;
                    return;
                }

                if (index >= _tail + _array.Length - _head)
                    throw new ArgumentOutOfRangeException();

                var offset = _head + index;
                if (offset < _array.Length)
                {
                    _array[offset] = value;
                }

                _array[offset%_array.Length] = value;
            }
        }

        /// <summary>
        /// Retrieves and removes the head of the queue represented by 
        /// this deque or returns default(T) if deque is empty.
        /// </summary>
        /// <returns></returns>
        public T Poll()
        {
            if (_head == _tail)
                return default(T);
            return RemoveFirst();
        }

        public T PopFront()
        {
            return RemoveFirst();
        }

        public T PopBack()
        {
            return RemoveLast();
        }

        /// <summary>
        /// Retrieves, but does not remove, the head of the queue represented by this deque, 
        /// or returns default(T) if this deque is empty.
        /// </summary>
        /// <returns></returns>
        public T Peek()
        {
            if (_head == _tail)
            {
                return default(T);
            }

            return First;
        }

        public T First
        {
            get
            {
                if (_head == _tail)
                {
                    throw new ArgumentOutOfRangeException();
                }

                int indx = _head;
                if (indx == _array.Length)
                    indx = 0;

                return _array[indx];
            }
        }

        public T Last
        {
            get
            {
                if (_head == _tail)
                {
                    throw new ArgumentOutOfRangeException();
                }

                int indx = _tail - 1;
                if (indx == -1)
                    indx = _array.Length - 1;

                return _array[indx];
            }
        }

        public int Count
        {
            get
            {
                if (_tail == _head)
                    return 0;
                if (_tail > _head)
                    return _tail - _head;
                return _tail + _array.Length - _head;
            }
        }

        public bool IsReadOnly => false;
    }
}
