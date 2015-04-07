///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.filter;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class contains the state of a single filter expression in the evaluation state tree.
    /// </summary>
    [Serializable]
    public sealed class EvalFilterStateNodeConsumeImpl 
        : EvalFilterStateNode
        , EvalFilterStateNodeConsume
    {
        public EvalFilterStateNodeConsumeImpl(Evaluator parentNode, EvalFilterNode evalFilterNode)
            : base(parentNode, evalFilterNode)
        {
        }

        public override void MatchFound(EventBean theEvent, ICollection<FilterHandleCallback> allStmtMatches)
        {
            // not receiving the remaining matches simply means we evaluate the event
            if (allStmtMatches == null)
            {
                base.MatchFound(theEvent, null);
                return;
            }

            EvalFilterConsumptionHandler handler = EvalFilterNode.Context.ConsumptionHandler;
            ProcessMatches(handler, theEvent, allStmtMatches);
        }

        public static void ProcessMatches(EvalFilterConsumptionHandler handler, EventBean theEvent, ICollection<FilterHandleCallback> allStmtMatches)
        {

            // ignore all other callbacks for the same event
            if (handler.LastEvent == theEvent)
            {
                return;
            }
            handler.LastEvent = theEvent;

            // evaluate consumption for all same-pattern filters
            var matches = new LinkedList<FilterHandleCallback>();

            var currentConsumption = int.MinValue;
            foreach (FilterHandleCallback callback in allStmtMatches)
            {
                if (!(callback is EvalFilterStateNodeConsume))
                {
                    continue;
                }
                var node = (EvalFilterStateNodeConsume)callback;
                var consumption = node.EvalFilterNode.FactoryNode.ConsumptionLevel;
                if (consumption == null)
                {
                    consumption = 0;
                }

                if (consumption > currentConsumption)
                {
                    matches.Clear();
                    currentConsumption = consumption.Value;
                }
                if (consumption == currentConsumption)
                {
                    matches.AddLast(callback);
                }
            }

            // execute matches
            foreach (FilterHandleCallback match in matches)
            {
                match.MatchFound(theEvent, null);
            }
        }
    }
}
