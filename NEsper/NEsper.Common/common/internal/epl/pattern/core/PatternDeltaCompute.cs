///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
#if DEPRECATED_INTERFACE
    public interface PatternDeltaCompute
    {
        long ComputeDelta(
            MatchedEventMap beginState,
            PatternAgentInstanceContext context);
    }
#else
    public delegate long PatternDeltaCompute(
        MatchedEventMap beginState,
        PatternAgentInstanceContext context);
#endif
} // end of namespace