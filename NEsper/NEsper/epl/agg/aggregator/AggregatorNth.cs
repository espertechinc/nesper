///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.agg.aggregator
{
    /// <summary>
    ///     Aggregator to return the Nth oldest element to enter, with N=1 the most recent
    ///     value is returned. If N is larger than the enter minus leave size, null is returned.
    ///     A maximum N historical values are stored, so it can be safely used to compare
    ///     recent values in large views without incurring excessive overhead.
    /// </summary>
    public class AggregatorNth : AggregationMethod
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="sizeBuf">size</param>
        public AggregatorNth(int sizeBuf)
        {
            SizeBuf = sizeBuf;
        }

        public int SizeBuf { get; }

        public object[] CircularBuffer { get; set; }

        public int CurrentBufferElementPointer { get; set; }

        public long NumDataPoints { get; set; }

        public virtual void Enter(object value)
        {
            var arr = (object[]) value;
            EnterValues(arr);
        }

        public virtual void Leave(object value)
        {
            if (SizeBuf > NumDataPoints)
            {
                var diff = SizeBuf - (int) NumDataPoints;
                CircularBuffer[(CurrentBufferElementPointer + diff - 1) % SizeBuf] = null;
            }

            NumDataPoints--;
        }

        public virtual object Value
        {
            get
            {
                if (CircularBuffer == null) return null;

                return CircularBuffer[(CurrentBufferElementPointer + SizeBuf) % SizeBuf];
            }
        }

        public virtual void Clear()
        {
            CircularBuffer = new object[SizeBuf];
            NumDataPoints = 0;
            CurrentBufferElementPointer = 0;
        }

        protected virtual void EnterValues(object[] arr)
        {
            NumDataPoints++;
            if (CircularBuffer == null) Clear();
            CircularBuffer[CurrentBufferElementPointer] = arr[0];
            CurrentBufferElementPointer = (CurrentBufferElementPointer + 1) % SizeBuf;
        }
    }
} // end of namespace