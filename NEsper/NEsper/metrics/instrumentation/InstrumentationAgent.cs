///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.metrics.instrumentation
{
    public interface InstrumentationAgent
    {
        void IndicateQ();
        void IndicateA();
    }

    public class ProxyInstrumentationAgent : InstrumentationAgent
    {
        public Action ProcIndicateQ { get; set; }
        public Action ProcIndicateA { get; set; }

        public void IndicateQ() { ProcIndicateQ(); }
        public void IndicateA() { ProcIndicateA(); }
    }
}
