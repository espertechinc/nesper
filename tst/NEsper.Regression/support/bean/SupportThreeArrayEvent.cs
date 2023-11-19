///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportThreeArrayEvent
    {
        public SupportThreeArrayEvent(
            string id,
            int value,
            int[] intArray,
            long[] longArray,
            double[] doubleArray)
        {
            Id = id;
            Value = value;
            IntArray = intArray;
            LongArray = longArray;
            DoubleArray = doubleArray;
        }

        public string Id { get; }

        public int Value { get; }

        public int[] IntArray { get; }

        public long[] LongArray { get; }

        public double[] DoubleArray { get; }
    }
} // end of namespace