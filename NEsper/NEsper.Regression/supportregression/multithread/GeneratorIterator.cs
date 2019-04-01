///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.multithread
{
    public class GeneratorIterator : IEnumerator<object>
    {
        public static readonly GeneratorIteratorCallback DEFAULT_SUPPORTEBEAN_CB = 
            numEvent => new SupportBean(Convert.ToString(numEvent), numEvent);
    
        private readonly int _maxNumEvents;
        private readonly GeneratorIteratorCallback _callback;
    
        private int _numEvents;
        private object _current;

        public GeneratorIterator(int maxNumEvents, GeneratorIteratorCallback callback) {
            _maxNumEvents = maxNumEvents;
            _callback = callback;
            _current = null;
        }
    
        public GeneratorIterator(int maxNumEvents) {
            _maxNumEvents = maxNumEvents;
            _callback = DEFAULT_SUPPORTEBEAN_CB;
        }

        public void Dispose()
        {
        }

        public void Reset()
        {
            _current = null;
            _numEvents = 0;
        }

        public bool MoveNext() {
            if (_numEvents < _maxNumEvents) {
                _current = _callback.Invoke(_numEvents);
                _numEvents++;
                return true;
            }
            return false;
        }

        public object Current => _current;
    }
} // end of namespace
