///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>
    /// Interface for populating a join tuple result set from new data and old data for each stream.
    /// </summary>
    public interface JoinSetComposer
    {
        /// <summary>
        /// Returns true whether initialization events per stream to populate join indexes can be processed (init method).
        /// </summary>
        /// <value></value>
        bool AllowsInit { get; }

        /// <summary>
        /// Provides initialization events per stream to composer to populate join indexes, if required
        /// </summary>
        /// <param name="eventsPerStream">is an array of events for each stream, with null elements to indicate no events for a stream</param>
        /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
        void Init(EventBean[][] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext);

        /// <summary>Return join tuple result set from new data and old data for each stream. </summary>
        /// <param name="newDataPerStream">for each stream the event array (can be null).</param>
        /// <param name="oldDataPerStream">for each stream the event array (can be null).</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        /// <returns>join tuples</returns>
        UniformPair<ISet<MultiKey<EventBean>>> Join(EventBean[][] newDataPerStream, EventBean[][] oldDataPerStream, ExprEvaluatorContext exprEvaluatorContext);

        /// <summary>For use in iteration over join statements, this must build a join tuple result set from all events in indexes, executing query strategies for each. </summary>
        /// <returns>static join result</returns>
        ISet<MultiKey<EventBean>> StaticJoin();

        /// <summary>Dispose stateful index tables, if any. </summary>
        void Destroy();

        void VisitIndexes(StatementAgentInstancePostLoadIndexVisitor visitor);
    }
}
