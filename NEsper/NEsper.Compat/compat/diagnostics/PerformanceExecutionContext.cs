///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat.diagnostics
{
    public class PerformanceExecutionContext
    {
        public TimeSpan InitialUserTime;
        public TimeSpan InitialPrivTime;
        public TimeSpan InitialTotalTime;

        public TimeSpan CurrentUserTime;
        public TimeSpan CurrentPrivTime;
        public TimeSpan CurrentTotalTime;
    }
}