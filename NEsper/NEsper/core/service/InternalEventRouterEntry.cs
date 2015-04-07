///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Pre-Processing entry for routing an event internally.
    /// </summary>
    public class InternalEventRouterEntry
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="priority">priority of statement</param>
        /// <param name="drop">whether to drop the event if matched</param>
        /// <param name="optionalWhereClause">where clause, or null if none provided</param>
        /// <param name="assignments">event property assignments</param>
        /// <param name="writer">writes values to an event</param>
        /// <param name="wideners">for widening types to write</param>
        /// <param name="outputView">for indicating output</param>
        /// <param name="agentInstanceLock">The agent instance lock.</param>
        /// <param name="hasSubselect">if set to <c>true</c> [has subselect].</param>
        public InternalEventRouterEntry(int priority,
                                        bool drop,
                                        ExprNode optionalWhereClause,
                                        ExprNode[] assignments,
                                        EventBeanWriter writer,
                                        TypeWidener[] wideners,
                                        InternalRoutePreprocessView outputView,
                                        IReaderWriterLock agentInstanceLock,
                                        bool hasSubselect)
        {
            Priority = priority;
            IsDrop = drop;
            OptionalWhereClause = optionalWhereClause == null ? null : optionalWhereClause.ExprEvaluator;
            Assignments = ExprNodeUtility.GetEvaluators(assignments);
            Writer = writer;
            Wideners = wideners;
            OutputView = outputView;
            AgentInstanceLock = agentInstanceLock;
            HasSubselect = hasSubselect;
        }

        /// <summary>Returns the execution priority. </summary>
        /// <value>priority</value>
        public int Priority { get; private set; }

        /// <summary>Returns indicator whether dropping events if the where-clause matches. </summary>
        /// <value>drop events</value>
        public bool IsDrop { get; private set; }

        /// <summary>Returns the where-clause or null if none defined </summary>
        /// <value>where-clause</value>
        public ExprEvaluator OptionalWhereClause { get; private set; }

        /// <summary>Returns the expressions providing values for assignment. </summary>
        /// <value>assignment expressions</value>
        public ExprEvaluator[] Assignments { get; private set; }

        /// <summary>Returns the writer to the event for writing property values. </summary>
        /// <value>writer</value>
        public EventBeanWriter Writer { get; private set; }

        /// <summary>Returns the type wideners to use or null if none required. </summary>
        /// <value>wideners.</value>
        public TypeWidener[] Wideners { get; private set; }

        /// <summary>Returns the output view. </summary>
        /// <value>output view</value>
        public InternalRoutePreprocessView OutputView { get; private set; }

        public IReaderWriterLock AgentInstanceLock { get; private set; }

        public bool HasSubselect { get; private set; }
    }
}