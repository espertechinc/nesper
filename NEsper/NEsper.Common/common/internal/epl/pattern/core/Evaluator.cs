///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    /// <summary>
    ///     Interface for nodes in an expression evaluation state tree that are being informed by a child that the
    ///     event expression fragments (subtrees) which the child represents has turned true (evaluateTrue method)
    ///     or false (evaluateFalse).
    /// </summary>
    public interface Evaluator
    {
        /// <summary>
        ///     Indicate a change in truth value to true.
        /// </summary>
        /// <param name="matchEvent">is the container for events that caused the change in truth value</param>
        /// <param name="fromNode">is the node that indicates the change</param>
        /// <param name="isQuitted">is an indication of whether the node continues listening or stops listening</param>
        /// <param name="optionalTriggeringEvent">
        ///     in case the truth value changed to true in direct response to an event arriving,
        ///     provides that event
        /// </param>
        void EvaluateTrue(
            MatchedEventMap matchEvent,
            EvalStateNode fromNode,
            bool isQuitted,
            EventBean optionalTriggeringEvent);

        /// <summary>
        ///     Indicate a change in truth value to false.
        /// </summary>
        /// <param name="fromNode">is the node that indicates the change</param>
        /// <param name="restartable">whether the evaluator can be restarted</param>
        void EvaluateFalse(
            EvalStateNode fromNode,
            bool restartable);

        bool IsFilterChildNonQuitting { get; }
    }
} // end of namespace