///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.agg.aggregator
{
    /// <summary>
    /// Aggregator to return the Nth oldest element to enter, with Count=1 the most recent value 
    /// is returned. If Count is larger than the enter minus leave size, null is returned. A maximum 
    /// Count historical values are stored, so it can be safely used to compare recent values in large 
    /// views without incurring excessive overhead.
    /// </summary>
    public class AggregatorNth : AggregationMethod
    {
        private readonly Type _returnType;
        private readonly int _sizeBuf;

        private Object[] _circularBuffer;
        private int _currentBufferElementPointer;
        private long _numDataPoints;
    
        /// <summary>Ctor. </summary>
        /// <param name="returnType">return type</param>
        /// <param name="sizeBuf">size</param>
        public AggregatorNth(Type returnType, int sizeBuf)
        {
            _returnType = returnType;
            _sizeBuf = sizeBuf;
        }
    
        public void Enter(Object value)
        {
            var arr = (Object[]) value;
            _numDataPoints++;
            if (_circularBuffer == null)
            {
                Clear();
            }
            _circularBuffer[_currentBufferElementPointer] = arr[0];
            _currentBufferElementPointer = (_currentBufferElementPointer + 1) % _sizeBuf;
        }
    
        public void Leave(Object value)
        {
            if (_sizeBuf > _numDataPoints)
            {
                int diff = _sizeBuf - (int) _numDataPoints;
                _circularBuffer[(_currentBufferElementPointer + diff - 1) % _sizeBuf] = null;
            }
            _numDataPoints--;
        }

        public Type ValueType
        {
            get { return _returnType; }
        }

        public object Value
        {
            get
            {
                if (_circularBuffer == null)
                {
                    return null;
                }
                return _circularBuffer[(_currentBufferElementPointer + _sizeBuf)%_sizeBuf];
            }
        }

        public void Clear()
        {
            _circularBuffer = new Object[_sizeBuf];
            _numDataPoints = 0;
            _currentBufferElementPointer = 0;
        }
    }
}
