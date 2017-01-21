///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.util;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// Interface for executing a Stop on an active event expression.
    /// </summary>
    public interface PatternStopCallback : StopCallback
    {
    }

    public class ProxyPatternStopCallback : ProxyStopCallback, PatternStopCallback
    {
        public ProxyPatternStopCallback() { }
        public ProxyPatternStopCallback(Action procStop) : base(procStop) { }
    }
}
