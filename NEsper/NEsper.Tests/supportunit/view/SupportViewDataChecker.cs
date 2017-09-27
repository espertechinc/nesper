///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;


namespace com.espertech.esper.supportunit.view
{
    public class SupportViewDataChecker {
        /// <summary>Compare the new data underlying events underlying events captured by the child against expected values in the exact same order. Clears the last new data in the test child view after comparing.  </summary>
        /// <param name="testChildView">is the child view</param>
        /// <param name="expectedValues">are the expected values</param>
        public static void CheckNewDataUnderlying(SupportBaseView testChildView, EventBean[] expectedValues)
        {
            EventBean[] newData = testChildView.LastNewData;
            Object[] expectedUnderlying = GetUnderlying(expectedValues);
            Object[] newUnderlying = GetUnderlying(newData);
            EPAssertionUtil.AssertEqualsExactOrder(expectedUnderlying, newUnderlying);
            testChildView.ClearLastNewData();
        }
    
        /// <summary>Compare the new data captured by the child against expected values in the exact same order. Clears the last new data in the test child view after comparing.  </summary>
        /// <param name="testChildView">is the child view</param>
        /// <param name="expectedValues">are the expected values</param>
        public static void CheckNewData(SupportBaseView testChildView, EventBean[] expectedValues) {
            EventBean[] newData = testChildView.LastNewData;
            EPAssertionUtil.AssertEqualsExactOrder(expectedValues, newData);
            testChildView.ClearLastNewData();
        }
    
        /// <summary>Compare the old data captured by the child against expected values in the exact same order. Clears the last old data in the test child view after comparing.  </summary>
        /// <param name="testChildView">is the child view</param>
        /// <param name="expectedValues">are the expected values</param>
        public static void CheckOldData(SupportBaseView testChildView, EventBean[] expectedValues) {
            EventBean[] oldData = testChildView.LastOldData;
            EPAssertionUtil.AssertEqualsExactOrder(expectedValues, oldData);
            testChildView.ClearLastOldData();
        }
    
        /// <summary>Compare the old data underlying object captured by the child against expected values in the exact same order. Clears the last old data in the test child view after comparing.  </summary>
        /// <param name="testChildView">is the child view</param>
        /// <param name="expectedValues">are the expected values</param>
        public static void CheckOldDataUnderlying(SupportBaseView testChildView, EventBean[] expectedValues) {
            EventBean[] oldData = testChildView.LastOldData;
            Object[] expectedUnderlying = GetUnderlying(expectedValues);
            Object[] oldUnderlying = GetUnderlying(oldData);
            EPAssertionUtil.AssertEqualsExactOrder(expectedUnderlying, oldUnderlying);
            testChildView.ClearLastOldData();
        }
    
        /// <summary>Compare the new data captured by the child against expected values in the exact same order. Clears the last new data in the test child view after comparing.  </summary>
        /// <param name="updateListener">is the Update listener caching the results</param>
        /// <param name="expectedValues">are the expected values</param>
        public static void CheckNewData(SupportUpdateListener updateListener, EventBean[] expectedValues) {
            EventBean[] newData = updateListener.LastNewData;
            EPAssertionUtil.AssertEqualsExactOrder(expectedValues, newData);
            updateListener.LastNewData = null;
        }
    
        /// <summary>Compare the new data captured by the child against expected values in the exact same order. Clears the last new data in the test child view after comparing.  </summary>
        /// <param name="updateListener">is the Update listener caching the results</param>
        /// <param name="expectedValues">are the expected values</param>
        public static void CheckNewDataUnderlying(SupportUpdateListener updateListener, EventBean[] expectedValues) {
            EventBean[] newData = updateListener.LastNewData;
            Object[] expectedUnderlying = GetUnderlying(expectedValues);
            Object[] newUnderlying = GetUnderlying(newData);
            EPAssertionUtil.AssertEqualsExactOrder(expectedUnderlying, newUnderlying);
            updateListener.LastNewData = null;
        }
    
        /// <summary>Compare the old data captured by the child against expected values in the exact same order. Clears the last old data in the test child view after comparing.  </summary>
        /// <param name="updateListener">is the Update listener caching the results</param>
        /// <param name="expectedValues">are the expected values</param>
        public static void CheckOldData(SupportUpdateListener updateListener, EventBean[] expectedValues) {
            EventBean[] oldData = updateListener.LastOldData;
            EPAssertionUtil.AssertEqualsExactOrder(expectedValues, oldData);
            updateListener.LastOldData = null;
        }
    
        private static Object[] GetUnderlying(EventBean[] events) {
            if (events == null) {
                return null;
            }
            Object[] underlying = new Object[events.Length];
            for (int i = 0; i < events.Length; i++) {
                underlying[i] = events[i].Underlying;
            }
            return underlying;
        }
    }
}
