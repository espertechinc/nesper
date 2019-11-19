///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public partial class EPLJoin2StreamRangePerformance
    {
        public class ProxyAssertionCallback : AssertionCallback
        {
            public Func<int, object> ProcGetEvent { get; set; }

            public Func<int, object[]> ProcGetExpectedValue { get; set; }

            public object GetEvent(int iteration)
            {
                return ProcGetEvent?.Invoke(iteration);
            }

            public object[] GetExpectedValue(int iteration)
            {
                return ProcGetExpectedValue?.Invoke(iteration);
            }
        }
    }
}