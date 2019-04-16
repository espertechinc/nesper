///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.epl.approx.countminsketch
{
    public class CountMinSketchStateTopk
    {
        private readonly IDictionary<ByteBuffer, long> _lastFreqForItem;

        // Wherein: Object either is ByteBuffer or Deque<ByteBuffer>

        public CountMinSketchStateTopk(int topkMax)
        {
            TopkMax = topkMax;
            _lastFreqForItem = new Dictionary<ByteBuffer, long>();
            Topk = new OrderedDictionary<long, object>(SimpleComparer<long>.Reverse);
        }

        public CountMinSketchStateTopk(
            int topkMax,
            OrderedDictionary<long, object> topk,
            IDictionary<ByteBuffer, long> lastFreqForItem)
        {
            TopkMax = topkMax;
            Topk = topk;
            _lastFreqForItem = lastFreqForItem;
        }

        public OrderedDictionary<long, object> Topk { get; }

        public IList<ByteBuffer> TopKValues {
            get {
                IList<ByteBuffer> values = new List<ByteBuffer>();
                foreach (var entry in Topk) {
                    if (entry.Value is Deque<ByteBuffer> dequeBuffer) {
                        foreach (var o in dequeBuffer) {
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

        public int TopkMax { get; }

        public void UpdateExpectIncreasing(
            byte[] value,
            long frequency)
        {
            var filled = _lastFreqForItem.Count == TopkMax;
            if (!filled) {
                var valueBuffer = new ByteBuffer(value);
                UpdateInternal(valueBuffer, frequency);
            }
            else {
                var lastKey = Topk.Last().Key;
                if (frequency > lastKey) {
                    var valueBuffer = new ByteBuffer(value);
                    UpdateInternal(valueBuffer, frequency);
                }
            }

            TrimItems();
        }

        private void UpdateInternal(
            ByteBuffer valueBuffer,
            long frequency)
        {
            if (_lastFreqForItem.Remove(valueBuffer, out var previousUpdateFrequency)) {
                RemoveItemFromSorted(previousUpdateFrequency, valueBuffer);
            }

            AddItemToSorted(frequency, valueBuffer);
        }

        private void RemoveItemFromSorted(
            long frequency,
            ByteBuffer value)
        {
            var existing = Topk.Get(frequency);
            if (existing is Deque<ByteBuffer> deque) {
                deque.Remove(value);
                if (deque.IsEmpty()) {
                    Topk.Remove(frequency);
                }
            }
            else if (existing != null) {
                Topk.Remove(frequency);
            }
        }

        private void AddItemToSorted(
            long frequency,
            ByteBuffer value)
        {
            var existing = Topk.Get(frequency);
            if (existing == null) {
                Topk.Put(frequency, value);
            }
            else if (existing is Deque<ByteBuffer> existingDeque) {
                existingDeque.Add(value);
            }
            else {
                Deque<ByteBuffer> deque = new ArrayDeque<ByteBuffer>(2);
                deque.Add((ByteBuffer) existing);
                deque.Add(value);
                Topk.Put(frequency, deque);
            }
        }

        private void TrimItems()
        {
            while (_lastFreqForItem.Count > TopkMax) {
                if (Topk.Count == 0) {
                    break;
                }

                var last = Topk.LastOrDefault();
                if (last.Value is Deque<ByteBuffer> deque) {
                    var valueRemoved = deque.RemoveLast();
                    _lastFreqForItem.Remove(valueRemoved);
                    if (deque.IsEmpty()) {
                        Topk.Remove(last.Key);
                    }
                }
                else {
                    Topk.Remove(last.Key);
                    _lastFreqForItem.Remove((ByteBuffer) last.Value);
                }
            }
        }
    }
}