///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.collection
{
    public class RollingTwoValueBuffer<TA, TB>
    {
        private int _nextFreeIndex;

        public RollingTwoValueBuffer(
            TA[] bufferA,
            TB[] bufferB)
        {
            if (bufferA.Length != bufferB.Length || bufferA.Length == 0) {
                throw new ArgumentException("Minimum buffer size is 1, buffer sizes must be identical");
            }

            BufferA = bufferA;
            BufferB = bufferB;
            _nextFreeIndex = 0;
        }

        public TA[] BufferA { get; private set; }

        public TB[] BufferB { get; private set; }

        public void Add(
            TA valueA,
            TB valueB)
        {
            BufferA[_nextFreeIndex] = valueA;
            BufferB[_nextFreeIndex] = valueB;
            _nextFreeIndex++;

            if (_nextFreeIndex == BufferA.Length) {
                _nextFreeIndex = 0;
            }
        }
    }
} // end of namespace