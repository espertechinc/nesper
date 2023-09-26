///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.aifactory.update;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.context.util
{
    /// <summary>
    ///     Pre-Processing entry for routing an event internally.
    /// </summary>
    public class InternalEventRouterEntry
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="priority">priority of statement</param>
        /// <param name="drop">whether to drop the event if matched</param>
        /// <param name="optionalWhereClause">where clause, or null if none provided</param>
        /// <param name="assignments">event property assignments</param>
        /// <param name="writer">writes values to an event</param>
        /// <param name="wideners">for widening types to write</param>
        /// <param name="OutputView">for indicating output</param>
        /// <param name="statementContext">the statement context</param>
        /// <param name="hasSubselect">indicator whether there are subselects</param>
        public InternalEventRouterEntry(
            int priority,
            bool drop,
            ExprEvaluator optionalWhereClause,
            ExprEvaluator[] assignments,
            EventBeanWriter writer,
            TypeWidener[] wideners,
            InternalEventRouterWriter[] specialPropWriters,
            InternalRoutePreprocessView outputView,
            StatementContext statementContext,
            bool hasSubselect)
        {
            Priority = priority;
            IsDrop = drop;
            OptionalWhereClause = optionalWhereClause;
            Assignments = assignments;
            Writer = writer;
            Wideners = wideners;
            SpecialPropWriters = specialPropWriters;
            OutputView = outputView;
            StatementContext = statementContext;
            IsSubselect = hasSubselect;
        }

        /// <summary>
        ///     Returns the execution priority.
        /// </summary>
        /// <returns>priority</returns>
        public int Priority { get; }

        /// <summary>
        ///     Returns indicator whether dropping events if the where-clause matches.
        /// </summary>
        /// <returns>drop events</returns>
        public bool IsDrop { get; }

        /// <summary>
        ///     Returns the where-clause or null if none defined
        /// </summary>
        /// <returns>where-clause</returns>
        public ExprEvaluator OptionalWhereClause { get; }

        /// <summary>
        ///     Returns the expressions providing values for assignment.
        /// </summary>
        /// <returns>assignment expressions</returns>
        public ExprEvaluator[] Assignments { get; }

        /// <summary>
        ///     Returns the writer to the event for writing property values.
        /// </summary>
        /// <returns>writer</returns>
        public EventBeanWriter Writer { get; }

        /// <summary>
        ///     Returns the type wideners to use or null if none required.
        /// </summary>
        /// <returns>wideners.</returns>
        public TypeWidener[] Wideners { get; }

        public InternalEventRouterWriter[] SpecialPropWriters { get; }

        /// <summary>
        ///     Returns the output view.
        /// </summary>
        /// <returns>output view</returns>
        public InternalRoutePreprocessView OutputView { get; }

        public StatementContext StatementContext { get; }

        public bool IsSubselect { get; }
    }
} // end of namespace