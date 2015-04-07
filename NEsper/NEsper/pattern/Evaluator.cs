///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// Interface for nodes in an expression evaluation state tree that are being informed by a child that the 
    /// event expression fragments (subtrees) which the child represents has turned true (evaluateTrue method) 
    /// or false (evaluateFalse).
    /// </summary>
    public interface Evaluator
    {
        /// <summary>
        /// Indicate a change in truth value to true.
        /// </summary>
        /// <param name="matchEvent">is the container for events that caused the change in truth value</param>
        /// <param name="fromNode">is the node that indicates the change</param>
        /// <param name="isQuitted">is an indication of whether the node continues listenening or stops listening</param>
        void EvaluateTrue(MatchedEventMap matchEvent, EvalStateNode fromNode, bool isQuitted);

        /// <summary>
        /// Indicate a change in truth value to false.
        /// </summary>
        /// <param name="fromNode">is the node that indicates the change</param>
        /// <param name="restartable">if set to <c>true</c> [restartable].</param>
        void EvaluateFalse(EvalStateNode fromNode, bool restartable);

        bool IsFilterChildNonQuitting { get; }
    }
}
