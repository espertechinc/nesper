///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.join.@base;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.statement.dispatch;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;

namespace com.espertech.esper.common.@internal.epl.output.core
{
    public class OutputStrategyUtil
    {
        public static void Output(
            bool forceUpdate,
            UniformPair<EventBean[]> result,
            UpdateDispatchView finalView)
        {
            var newEvents = result != null ? result.First : null;
            var oldEvents = result != null ? result.Second : null;
            if (newEvents != null || oldEvents != null) {
                finalView.NewResult(result);
            }
            else if (forceUpdate) {
                finalView.NewResult(result);
            }
        }

        /// <summary>
        ///     Indicate statement result.
        /// </summary>
        /// <param name="newOldEvents">result</param>
        /// <param name="statementContext">context</param>
        public static void IndicateEarlyReturn(
            StatementContext statementContext,
            UniformPair<EventBean[]> newOldEvents)
        {
            // no action
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="joinExecutionStrategy">join strategy</param>
        /// <param name="resultSetProcessor">processor</param>
        /// <param name="parentView">view</param>
        /// <param name="distinct">flag</param>
        /// <param name="distinctKeyGetter"></param>
        /// <returns>iterator</returns>
        public static IEnumerator<EventBean> GetEnumerator(
            JoinExecutionStrategy joinExecutionStrategy,
            ResultSetProcessor resultSetProcessor,
            Viewable parentView,
            bool distinct,
            EventPropertyValueGetter distinctKeyGetter)
        {
            IEnumerator<EventBean> enumerator;
            if (joinExecutionStrategy != null) {
                var joinSet = joinExecutionStrategy.StaticJoin();
                enumerator = resultSetProcessor.GetEnumerator(joinSet);
            }
            else if (resultSetProcessor != null) {
                enumerator = resultSetProcessor.GetEnumerator(parentView);
            }
            else {
                enumerator = parentView.GetEnumerator();
            }

            if (!distinct) {
                return enumerator;
            }

            return EventDistinctEnumerator.For(enumerator, distinctKeyGetter);
        }
    }
} // end of namespace