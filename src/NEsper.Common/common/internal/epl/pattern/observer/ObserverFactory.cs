///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.epl.pattern.observer
{
    /// <summary>
    ///     Interface for factories for making observer instances.
    /// </summary>
    public interface ObserverFactory
    {
        bool IsNonRestarting { get; }

        /// <summary>
        ///     Make an observer instance.
        /// </summary>
        /// <param name="context">services that may be required by observer implementation</param>
        /// <param name="beginState">start state for observer</param>
        /// <param name="observerEventEvaluator">receiver for events observed</param>
        /// <param name="observerState">state node for observer</param>
        /// <param name="isFilterChildNonQuitting">true for non-quitting filter</param>
        /// <returns>observer instance</returns>
        EventObserver MakeObserver(
            PatternAgentInstanceContext context,
            MatchedEventMap beginState,
            ObserverEventEvaluator observerEventEvaluator,
            object observerState,
            bool isFilterChildNonQuitting);
    }
} // end of namespace