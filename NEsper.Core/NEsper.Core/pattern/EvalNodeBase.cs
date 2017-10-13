///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// Superclass of all nodes in an evaluation tree representing an event pattern expression.
    /// Follows the Composite pattern. Child nodes do not carry references to parent nodes, the 
    /// tree is unidirectional.
    /// </summary>
    [Serializable]
    public abstract class EvalNodeBase : EvalNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EvalNodeBase"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        protected EvalNodeBase(PatternAgentInstanceContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Create the evaluation state node containing the truth value state for each operator in an
        /// event expression.
        /// </summary>
        /// <param name="parentNode">the parent evaluator node that this node indicates a change in truth value to</param>
        /// <param name="stateNodeNumber">The state node number.</param>
        /// <param name="stateNodeId">the new state object's identifier</param>
        /// <returns>
        /// state node containing the truth value state for the operator
        /// </returns>
        public abstract EvalStateNode NewState(Evaluator parentNode,
                                               EvalStateNodeNumber stateNodeNumber,
                                               long stateNodeId);

        public virtual PatternAgentInstanceContext Context { get; private set; }
    }
}
