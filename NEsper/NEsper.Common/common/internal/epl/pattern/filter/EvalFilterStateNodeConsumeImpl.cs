///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.pattern.filter
{
    /// <summary>
    /// This class contains the state of a single filter expression in the evaluation state tree.
    /// </summary>
    public class EvalFilterStateNodeConsumeImpl : EvalFilterStateNode,
        EvalFilterStateNodeConsume
    {
        public EvalFilterStateNodeConsumeImpl(
            Evaluator parentNode,
            EvalFilterNode evalFilterNode)
            : base(
                parentNode,
                evalFilterNode)
        {
        }

        public override void MatchFound(
            EventBean theEvent,
            ICollection<FilterHandleCallback> allStmtMatches)
        {
            // not receiving the remaining matches simply means we evaluate the event
            if (allStmtMatches == null) {
                base.MatchFound(theEvent, null);
                return;
            }

            EvalFilterConsumptionHandler handler = EvalFilterNode.Context.ConsumptionHandler;
            ProcessMatches(handler, theEvent, allStmtMatches);
        }

        private static void ProcessMatches(
            EvalFilterConsumptionHandler handler,
            EventBean theEvent,
            ICollection<FilterHandleCallback> allStmtMatches)
        {
            // ignore all other callbacks for the same event
            if (handler.LastEvent == theEvent) {
                return;
            }

            handler.LastEvent = theEvent;

            // evaluate consumption for all same-pattern filters
            ArrayDeque<FilterHandleCallback> matches = new ArrayDeque<FilterHandleCallback>();

            int currentConsumption = int.MinValue;
            foreach (FilterHandleCallback callback in allStmtMatches) {
                if (!(callback is EvalFilterStateNodeConsume)) {
                    continue;
                }

                EvalFilterStateNodeConsume node = (EvalFilterStateNodeConsume) callback;
                int? consumption = node.EvalFilterNode.FactoryNode.ConsumptionLevel;
                if (consumption == null) {
                    consumption = 0;
                }

                if (consumption > currentConsumption) {
                    matches.Clear();
                    currentConsumption = consumption.Value;
                }

                if (consumption == currentConsumption) {
                    matches.Add(callback);
                }
            }

            // execute matches
            foreach (FilterHandleCallback match in matches) {
                match.MatchFound(theEvent, null);
            }
        }
    }
} // end of namespace