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
    ///     For use by <seealso cref="EventObserver" /> instances to place an event for processing/evaluation.
    /// </summary>
    public interface ObserverEventEvaluator
    {
        PatternAgentInstanceContext Context { get; }

        /// <summary>
        ///     Indicate an event for evaluation (sub-expression the observer represents has turned true).
        /// </summary>
        /// <param name="matchEvent">is the matched events so far</param>
        /// <param name="quitted">whether the observer quit, usually "true" for most observers</param>
        void ObserverEvaluateTrue(
            MatchedEventMap matchEvent,
            bool quitted);

        /// <summary>
        ///     Indicate that the observer turned permanently false.
        /// </summary>
        /// <param name="restartable">true for whether it can restart</param>
        void ObserverEvaluateFalse(bool restartable);
    }
} // end of namespace