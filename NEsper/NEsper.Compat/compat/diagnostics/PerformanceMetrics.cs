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
    public struct PerformanceMetrics
    {
        public TimeSpan UserTime;
        public TimeSpan PrivTime;
        public TimeSpan TotalTime;
        public int NumInput;

        public PerformanceMetrics(
            TimeSpan userTime,
            TimeSpan privTime,
            TimeSpan totalTime,
            int numInput)
        {
            UserTime = userTime;
            PrivTime = privTime;
            TotalTime = totalTime;
            NumInput = numInput;
        }
    }
}