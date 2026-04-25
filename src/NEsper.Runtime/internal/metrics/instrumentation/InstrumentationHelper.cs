///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.@internal.metrics.instrumentation
{
    public class InstrumentationHelper
    {
#if DEBUG
        public const bool ENABLED = true;
        public const bool ASSERTIONENABLED = true;
#else
        public const bool ENABLED = false;
        public const bool ASSERTIONENABLED = false;
#endif

        public static readonly Instrumentation DEFAULT_INSTRUMENTATION = InstrumentationDefault.INSTANCE;
        public static Instrumentation instrumentation = DEFAULT_INSTRUMENTATION;

        public static Instrumentation Get()
        {
            return instrumentation;
        }
    }
} // end of namespace