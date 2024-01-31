///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.time.abacus
{
    public class TimeAbacusFactory
    {
        public static TimeAbacus Make(TimeUnit timeUnit)
        {
            if (timeUnit == TimeUnit.MILLISECONDS) {
                return TimeAbacusMilliseconds.INSTANCE;
            }
            else if (timeUnit == TimeUnit.MICROSECONDS) {
                return TimeAbacusMicroseconds.INSTANCE;
            }
            else {
                throw new ConfigurationException(
                    "Invalid time-source time unit of " + timeUnit + ", expected millis or micros");
            }
        }
    }
} // end of namespace