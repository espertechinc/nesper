///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.supportregression.bean;


namespace com.espertech.esper.supportregression.epl
{
    public class SupportStaticMethodInvocations
    {
        private static readonly IList<String> Invocations = new List<String>();

        public static int GetInvocationSizeAndReset()
        {
            int size = Invocations.Count;
            Invocations.Clear();
            return size;
        }

        public static SupportBean_S0 FetchObjectLog(String fetchId, int passThroughNumber)
        {
            Invocations.Add(fetchId);
            return new SupportBean_S0(passThroughNumber, "|" + fetchId + "|");
        }
    }
}
