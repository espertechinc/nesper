///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


namespace com.espertech.esper.common.@internal.epl.agg.method.nth
{
    public class AggregationNthState
    {
        private object[] circularBuffer;
        private int currentBufferElementPointer;
        private long numDataPoints;

        public object[] CircularBuffer {
            get => circularBuffer;

            set => circularBuffer = value;
        }

        public int CurrentBufferElementPointer {
            get => currentBufferElementPointer;

            set => currentBufferElementPointer = value;
        }

        public long NumDataPoints {
            get => numDataPoints;

            set => numDataPoints = value;
        }
    }
} // end of namespace