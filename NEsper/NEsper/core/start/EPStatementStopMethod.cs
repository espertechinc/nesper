///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.core.start
{
    public interface EPStatementStopMethod
    {
        void Stop();
    }

    public class ProxyEPStatementStopMethod : EPStatementStopMethod
    {
        public Action ProcStop;

        public ProxyEPStatementStopMethod() { }
        public ProxyEPStatementStopMethod(Action procStop)
        {
            ProcStop = procStop;
        }

        public void Stop()
        {
            ProcStop.Invoke();
        }
    }
}
