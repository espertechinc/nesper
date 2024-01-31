///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;

namespace com.espertech.esper.regressionlib.support.epl
{
    public class SupportStaticMethodInvocations
    {
        private static readonly IList<string> invocations = new List<string>();

        public static int GetInvocationSizeReset()
        {
            var size = invocations.Count;
            invocations.Clear();
            return size;
        }

        public static SupportBean_S0 FetchObjectLog(
            string fetchId,
            int passThroughNumber)
        {
            invocations.Add(fetchId);
            return new SupportBean_S0(passThroughNumber, "|" + fetchId + "|");
        }
    }
} // end of namespace