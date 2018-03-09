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

namespace com.espertech.esper.epl.approx
{
    public class CountMinSketchStateTopk
    {
        private readonly int _topkMax;

        // Wherein: Object either is Blob or Deque<Blob>
        private readonly SortedDictionary<long, object> _topk;
        private readonly IDictionary<Blob, long?> _lastFreqForItem;

        public CountMinSketchStateTopk(int topkMax)
        {
            _topkMax = topkMax;
            _lastFreqForItem = new Dictionary<Blob, long?>();
            _topk = new SortedDictionary<long, object>(SimpleComparer<long>.Reverse);
        }

        public CountMinSketchStateTopk(
            int topkMax,
            SortedDictionary<long, object> topk,
            IDictionary<Blob, long?> lastFreqForItem)
        {
            _topkMax = topkMax;
            _topk = topk;
            _lastFreqForItem = lastFreqForItem;
        }

        public SortedDictionary<long, object> Topk
        {
            get { return _topk; }
        }

        public void UpdateExpectIncreasing(byte[] value, long frequency)
        {
            var filled = _lastFreqForItem.Count == _topkMax;
            if (!filled)
            {
                var valueBuffer = new Blob(value);
                UpdateInternal(valueBuffer, frequency);
            }
            else
            {
                var lastKey = _topk.Last().Key;
                if (frequency > lastKey)
                {
                    var valueBuffer = new Blob(value);
                    UpdateInternal(valueBuffer, frequency);
                }
            }

            TrimItems();
        }

        private void UpdateInternal(Blob valueBuffer, long frequency)
        {
            var beforeUpdateFrequency = _lastFreqForItem.Push(valueBuffer, frequency);
            if (beforeUpdateFrequency != null)
            {
                RemoveItemFromSorted(beforeUpdateFrequency.Value, valueBuffer);
            }
            AddItemToSorted(frequency, valueBuffer);
        }

        private void RemoveItemFromSorted(long frequency, Blob value)
        {
            var existing = _topk.Get(frequency);
            if (existing is Deque<Blob> deque)
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

        private void AddItemToSorted(long frequency, Blob value)
        {
            var existing = _topk.Get(frequency);
            if (existing == null)
            {
                _topk.Put(frequency, value);
            }
            else if (existing is Deque<Blob>)
            {
                var deque = (Deque<Blob>)existing;
                deque.Add(value);
            }
            else
            {
                Deque<Blob> deque = new ArrayDeque<Blob>(2);
                deque.Add((Blob)existing);
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
                if (last.Value is Deque<Blob>)
                {
                    var deque = (Deque<Blob>)last.Value;
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
                    _lastFreqForItem.Remove((Blob)last.Value);
                }
            }
        }

        public IList<Blob> TopKValues
        {
            get
            {
                IList<Blob> values = new List<Blob>();
                foreach (var entry in _topk)
                {
                    if (entry.Value is Deque<Blob>)
                    {
                        var set = (Deque<Blob>)entry.Value;
                        foreach (var o in set)
                        {
                            values.Add(o);
                        }
                    }
                    else
                    {
                        values.Add((Blob)entry.Value);
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
