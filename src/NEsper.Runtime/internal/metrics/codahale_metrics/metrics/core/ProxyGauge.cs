///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.function;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core
{
    public class ProxyGauge<T> : Gauge<T>
    {
        public Supplier<T> ProcValue;
        public override T Value => ProcValue.Invoke();

        public ProxyGauge(Supplier<T> procValue)
        {
            ProcValue = procValue;
        }

        public ProxyGauge()
        {
        }
    }
}