///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.util
{
    /// <summary>
    /// Returns a {@code 1} the first time it's called, a {@code 0} every time after that.
    /// </summary>
    public class ToggleGauge : Gauge<long>
    {
        private readonly AtomicLong value = new AtomicLong(1);

        public override long Value
        {
            get {
                try
                {
                    return value.Get();
                }
                finally
                {
                    this.value.Set(0);
                }
            }
        }
    }
} // end of namespace