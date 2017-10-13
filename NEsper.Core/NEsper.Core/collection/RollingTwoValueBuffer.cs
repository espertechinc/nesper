///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.collection
{
    public class RollingTwoValueBuffer<A, B>
    {
        private int _nextFreeIndex;

        public RollingTwoValueBuffer(A[] bufferA, B[] bufferB)
        {
            if (bufferA.Length != bufferB.Length || bufferA.Length == 0)
            {
                throw new ArgumentException("Minimum buffer size is 1, buffer sizes must be identical");
            }

            BufferA = bufferA;
            BufferB = bufferB;
            _nextFreeIndex = 0;
        }

        public A[] BufferA { get; private set; }

        public B[] BufferB { get; private set; }

        public void Add(A valueA, B valueB)
        {
            BufferA[_nextFreeIndex] = valueA;
            BufferB[_nextFreeIndex] = valueB;
            _nextFreeIndex++;

            if (_nextFreeIndex == BufferA.Length)
            {
                _nextFreeIndex = 0;
            }
        }
    }
} // end of namespace