///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.epl.pattern.matchuntil
{
    public class EvalMatchUntilStateBounds
    {
        public EvalMatchUntilStateBounds(
            int? lowerbounds,
            int? upperbounds)
        {
            Lowerbounds = lowerbounds;
            Upperbounds = upperbounds;
        }

        public int? Lowerbounds { get; }

        public int? Upperbounds { get; }

        public static EvalMatchUntilStateBounds InitBounds(
            EvalMatchUntilFactoryNode factoryNode,
            MatchedEventMap beginState,
            PatternAgentInstanceContext context)
        {
            int? lowerbounds = null;
            int? upperbounds = null;

            var convertor = factoryNode.OptionalConvertor;
            var eventsPerStream = convertor == null ? null : convertor.Invoke(beginState);
            if (factoryNode.SingleBound != null) {
                var bounds = (int?) factoryNode.SingleBound.Evaluate(
                    eventsPerStream,
                    true,
                    context.AgentInstanceContext);
                lowerbounds = bounds;
                upperbounds = bounds;
            }
            else {
                if (factoryNode.LowerBounds != null) {
                    lowerbounds = (int?) factoryNode.LowerBounds.Evaluate(
                        eventsPerStream,
                        true,
                        context.AgentInstanceContext);
                }

                if (factoryNode.UpperBounds != null) {
                    upperbounds = (int?) factoryNode.UpperBounds.Evaluate(
                        eventsPerStream,
                        true,
                        context.AgentInstanceContext);
                }

                if (upperbounds != null && lowerbounds != null) {
                    if (upperbounds < lowerbounds) {
                        var lbounds = lowerbounds;
                        lowerbounds = upperbounds;
                        upperbounds = lbounds;
                    }
                }
            }

            return new EvalMatchUntilStateBounds(lowerbounds, upperbounds);
        }
    }
} // end of namespace