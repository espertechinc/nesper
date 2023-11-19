///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportEventWithIntArray
    {
        public SupportEventWithIntArray(
            string id,
            int[] array,
            int value)
        {
            Id = id;
            Array = array;
            Value = value;
        }

        public SupportEventWithIntArray(
            string id,
            int[] array) : this(id, array, 0)
        {
        }

        public int[] Array { get; }

        public string Id { get; }

        public int Value { get; }
    }
} // end of namespace