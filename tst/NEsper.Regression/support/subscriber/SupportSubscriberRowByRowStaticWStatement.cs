///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.support.subscriber
{
    public class SupportSubscriberRowByRowStaticWStatement
    {
        public static List<object[]> Indicate { get; } = new List<object[]>();

        public static List<EPStatement> Statements { get; } = new List<EPStatement>();

        public static void Update(
            EPStatement statement,
            string theString,
            int intPrimitive)
        {
            Indicate.Add(new object[] {theString, intPrimitive});
            Statements.Add(statement);
        }

        public void Reset()
        {
            Indicate.Clear();
            Statements.Clear();
        }
    }
} // end of namespace