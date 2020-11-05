///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.regressionlib.support.subscriber
{
    public class SupportSubscriberRowByRowStatic
    {
        private static List<object[]> indicate = new List<object[]>();

        public static void Update(
            string theString,
            int intPrimitive)
        {
            indicate.Add(new object[] {theString, intPrimitive});
        }

        public static IList<object[]> GetAndResetIndicate()
        {
            IList<object[]> result = indicate;
            indicate = new List<object[]>();
            return result;
        }
    }
} // end of namespace