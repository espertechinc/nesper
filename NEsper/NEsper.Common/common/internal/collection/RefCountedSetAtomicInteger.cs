///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.collection
{
    public class RefCountedSetAtomicInteger<K>
    {
        private readonly IDictionary<K, Object> _refs;
    
        public RefCountedSetAtomicInteger() {
            _refs = new Dictionary<K, Object>();
        }
    
        /// <summary>Clear out the collection. </summary>
        public void Clear()
        {
            _refs.Clear();
        }
    
        public bool Add(K key) {
            var count = _refs.Get(key);
            if (count == null)
            {
                _refs[key] = 1;
                return true;
            }
            else if (count is Mutable<int>)
            {
                var mutable = (Mutable<int>) count;
                Interlocked.Increment(ref mutable.Value);
                return false;
            }
            else
            {
                _refs[key] = new Mutable<int>(2);
                return false;
            }
        }
    
        public bool Remove(K key) {
            var count = _refs.Get(key);
            if (count == null) {
                return false;
            }
            else if (count is Mutable<int>) 
            {
                var mutable = (Mutable<int>)count;
                var val = Interlocked.Decrement(ref mutable.Value);
                if (val == 0) {
                    _refs.Remove(key);
                    return true;
                }
                return false;
            }
            else
            {
                _refs.Remove(key);
                return true;
            }
        }
    
        public void RemoveAll(K key)
        {
            _refs.Remove(key);
        }
    
        public bool IsEmpty() {
            return _refs.IsEmpty();
        }

        public IDictionary<K, object> Refs
        {
            get { return _refs; }
        }
    }
}
