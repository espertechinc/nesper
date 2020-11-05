///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    /// <summary>
    ///     Superclass of all state nodes in an evaluation node tree representing an event expressions.
    ///     Follows the Composite pattern. Subclasses are expected to keep their own collection containing child nodes
    ///     as needed.
    /// </summary>
    public abstract class EvalStateNode
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="parentNode">is the evaluator for this node on which to indicate a change in truth value</param>
        protected EvalStateNode(Evaluator parentNode)
        {
            ParentEvaluator = parentNode;
        }

        /// <summary>
        ///     Returns the factory node for the state node.
        /// </summary>
        /// <returns>factory node</returns>
        public abstract EvalNode FactoryNode { get; }

        public abstract bool IsNotOperator { get; }

        public abstract bool IsFilterStateNode { get; }

        public abstract bool IsObserverStateNodeNonRestarting { get; }

        /// <summary>
        ///     Returns the parent evaluator.
        /// </summary>
        /// <returns>parent evaluator instance</returns>
        public Evaluator ParentEvaluator { get; set; }

        /// <summary>
        ///     Starts the event expression or an instance of it.
        ///     Child classes are expected to initialize and start any event listeners
        ///     or schedule any time-based callbacks as needed.
        /// </summary>
        /// <param name="beginState">begin state</param>
        public abstract void Start(MatchedEventMap beginState);

        /// <summary>
        ///     Stops the event expression or an instance of it. Child classes are expected to free resources
        ///     and stop any event listeners or remove any time-based callbacks.
        /// </summary>
        public abstract void Quit();

        /// <summary>
        ///     Accept a visitor. Child classes are expected to invoke the visit method on the visitor instance
        ///     passed in.
        /// </summary>
        /// <param name="visitor">on which the visit method is invoked by each node</param>
        public abstract void Accept(EvalStateNodeVisitor visitor);

        /// <summary>
        ///     Remove matches that overlap with the provided events.
        /// </summary>
        /// <param name="matchEvent">set of events to check for</param>
        public abstract void RemoveMatch(ISet<EventBean> matchEvent);

        public virtual void Transfer(AgentInstanceTransferServices services)
        {
            // no action is the default
        }
    }
} // end of namespace