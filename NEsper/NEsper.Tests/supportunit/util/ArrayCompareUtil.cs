///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.client;

namespace com.espertech.esper.supportunit.util
{
    public class ArrayCompareUtil
    {
        /// <summary>
        /// Compare the events in the two object arrays assuming the exact same order.
        /// </summary>
        /// <param name="data">is the data to assertEqualsExactOrder against</param>
        /// <param name="expectedValues">is the expected values</param>
        public static bool CompareEqualsExactOrder(EventBean[] data, EventBean[] expectedValues)
        {
            if ((expectedValues == null) && (data == null))
            {
                return true;
            }
            if ( ((expectedValues == null) && (data != null)) ||
                 ((expectedValues != null) && (data == null)) )
            {
                return false;
            }
    
            if (expectedValues.Length != data.Length)
            {
                return false;
            }
    
            for (int i = 0; i < expectedValues.Length; i++)
            {
                if ((data[i] == null) && (expectedValues[i] == null))
                {
                    continue;
                }
    
                if (!data[i].Equals(expectedValues[i]))
                {
                    return false;
                }
            }
            return true;
        }
    
        /// <summary>
        /// Compare the objects in the two object arrays assuming the exact same order.
        /// </summary>
        /// <param name="data">is the data to assertEqualsExactOrder against</param>
        /// <param name="expectedValues">is the expected values</param>
        public static bool CompareRefExactOrder(Object[] data, Object[] expectedValues)
        {
            if ((expectedValues == null) && (data == null))
            {
                return true;
            }
            if ( ((expectedValues == null) && (data != null)) ||
                 ((expectedValues != null) && (data == null)) )
            {
                return false;
            }
    
            if (expectedValues.Length != data.Length)
            {
                return false;
            }
    
            for (int i = 0; i < expectedValues.Length; i++)
            {
                if (expectedValues[i] != data[i])
                {
                    return false;
                }
            }
    
            return true;
        }
    }
}
