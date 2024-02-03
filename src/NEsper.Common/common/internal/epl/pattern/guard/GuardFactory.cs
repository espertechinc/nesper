///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.epl.pattern.guard
{
    /// <summary>
    ///     Interface for a factory for <seealso cref="Guard" /> instances.
    /// </summary>
    public interface GuardFactory
    {
        /// <summary>
        ///     Constructs a guard instance.
        /// </summary>
        /// <param name="context">services for use by guard</param>
        /// <param name="beginState">the prior matching events</param>
        /// <param name="quitable">to use for indicating the guard has quit</param>
        /// <param name="guardState">state node for guard</param>
        /// <returns>guard instance</returns>
        Guard MakeGuard(
            PatternAgentInstanceContext context,
            MatchedEventMap beginState,
            Quitable quitable,
            object guardState);
    }
} // end of namespace