///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.epl.approx.countminsketch
{
    public class CountMinSketchStateTopk
    {
        private readonly int _topkMax;

        // Wherein: Object either is ByteBuffer or Deque<ByteBuffer>
        private readonly OrderedDictionary<long, object> _topk;
        private readonly IDictionary<ByteBuffer, long> _lastFreqForItem;

        public CountMinSketchStateTopk(int topkMax)
        {
            _topkMax = topkMax;
            _lastFreqForItem = new Dictionary<ByteBuffer, long>();
            _topk = new OrderedDictionary<long, object>(SimpleComparer<long>.Reverse);
        }

        public CountMinSketchStateTopk(
            int topkMax,
            OrderedDictionary<long, object> topk,
            IDictionary<ByteBuffer, long> lastFreqForItem)
        {
            _topkMax = topkMax;
            _topk = topk;
            _lastFreqForItem = lastFreqForItem;
        }

        public OrderedDictionary<long, object> Topk
        {
            get { return _topk; }
        }

        public void UpdateExpectIncreasing(byte[] value, long frequency)
        {
            var filled = _lastFreqForItem.Count == _topkMax;
            if (!filled)
            {
                var valueBuffer = new ByteBuffer(value);
                UpdateInternal(valueBuffer, frequency);
            }
            else
            {
                var lastKey = _topk.Last().Key;
                if (frequency > lastKey)
                {
                    var valueBuffer = new ByteBuffer(value);
                    UpdateInternal(valueBuffer, frequency);
                }
            }

            TrimItems();
        }

        private void UpdateInternal(ByteBuffer valueBuffer, long frequency)
        {
            var beforeUpdateFrequency = _lastFreqForItem.Push(valueBuffer, frequency);
            if (beforeUpdateFrequency != null)
            {
                RemoveItemFromSorted(beforeUpdateFrequency.Value, valueBuffer);
            }
            AddItemToSorted(frequency, valueBuffer);
        }

        private void RemoveItemFromSorted(long frequency, ByteBuffer value)
        {
            var existing = _topk.Get(frequency);
            if (existing is Deque<ByteBuffer> deque)
            {
                deque.Remove(value);
                if (deque.IsEmpty())
                {
                    _topk.Remove(frequency);
                }
            }
            else if (existing != null)
            {
                _topk.Remove(frequency);
            }
        }

        private void AddItemToSorted(long frequency, ByteBuffer value)
        {
            var existing = _topk.Get(frequency);
            if (existing == null)
            {
                _topk.Put(frequency, value);
            }
            else if (existing is Deque<ByteBuffer>)
            {
                var deque = (Deque<ByteBuffer>)existing;
                deque.Add(value);
            }
            else
            {
                Deque<ByteBuffer> deque = new ArrayDeque<ByteBuffer>(2);
                deque.Add((ByteBuffer)existing);
                deque.Add(value);
                _topk.Put(frequency, deque);
            }
        }

        private void TrimItems()
        {
            while (_lastFreqForItem.Count > _topkMax)
            {
                if (_topk.Count == 0)
                {
                    break;
                }

                var last = _topk.LastOrDefault();
                if (last.Value is Deque<ByteBuffer>)
                {
                    var deque = (Deque<ByteBuffer>)last.Value;
                    var valueRemoved = deque.RemoveLast();
                    _lastFreqForItem.Remove(valueRemoved);
                    if (deque.IsEmpty())
                    {
                        _topk.Remove(last.Key);
                    }
                }
                else
                {
                    _topk.Remove(last.Key);
                    _lastFreqForItem.Remove((ByteBuffer)last.Value);
                }
            }
        }

        public IList<ByteBuffer> TopKValues
        {
            get
            {
                IList<ByteBuffer> values = new List<ByteBuffer>();
                foreach (var entry in _topk)
                {
                    if (entry.Value is Deque<ByteBuffer>)
                    {
                        var set = (Deque<ByteBuffer>)entry.Value;
                        foreach (var o in set)
                        {
                            values.Add(o);
                        }
                    }
                    else {
                        values.Add((ByteBuffer) entry.Value);
                    }
                }
                return values;
            }
        }

        public int TopkMax
        {
            get { return _topkMax; }
        }
    }
}