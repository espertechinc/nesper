///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.prior
{
    /// <summary>
    ///     Factory for making <seealso cref="PriorEventView" /> instances.
    /// </summary>
    public class PriorEventViewFactory : ViewFactory
    {
        protected internal EventType eventType;

        /// <summary>
        ///     unbound to indicate the we are not receiving remove stream events (unbound stream, stream without child
        ///     views) therefore must use a different buffer.
        /// </summary>
        protected internal bool isUnbound;

        public bool IsUnbound {
            set => isUnbound = value;
        }

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            return new PriorEventView(agentInstanceViewFactoryContext.PriorViewUpdatedCollection);
        }

        public EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public string ViewName => "prior";

        public ViewUpdatedCollection MakeViewUpdatedCollection(
            SortedSet<int> priorRequests,
            AgentInstanceContext agentInstanceContext)
        {
            if (priorRequests.IsEmpty()) {
                throw new IllegalStateException("No resources requested");
            }

            // Construct an array of requested prior-event indexes (such as 10th prior event, 8th prior = {10, 8})
            var requested = new int[priorRequests.Count];
            var count = 0;
            foreach (var reqIndex in priorRequests) {
                requested[count++] = reqIndex;
            }

            // For unbound streams the buffer is strictly rolling new events
            if (isUnbound) {
                return new PriorEventBufferUnbound(priorRequests.Last());
            }

            if (requested.Length == 1) {
                // For bound streams (with views posting old and new data), and if only one prior index requested
                return new PriorEventBufferSingle(requested[0]);
            }

            // For bound streams (with views posting old and new data)
            // Multiple prior event indexes requested, such as "prior(2, price), prior(8, price)"
            // Sharing a single viewUpdatedCollection for multiple prior-event indexes
            return new PriorEventBufferMulti(requested);
        }
    }
} // end of namespace