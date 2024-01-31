///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.aifactory.createcontext;
using com.espertech.esper.common.@internal.context.aifactory.createtable;
using com.espertech.esper.common.@internal.context.aifactory.createwindow;
using com.espertech.esper.common.@internal.context.aifactory.select;
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.statement.resource
{
    public class StatementResourceHolderUtil
    {
        public static StatementResourceHolder PopulateHolder(
            AgentInstanceContext agentInstanceContext,
            StatementAgentInstanceFactoryResult startResult)
        {
            var holder = new StatementResourceHolder(
                agentInstanceContext,
                startResult.StopCallback,
                startResult.FinalView,
                startResult.OptionalAggegationService,
                startResult.PriorStrategies,
                startResult.PreviousGetterStrategies,
                startResult.RowRecogPreviousStrategy);
            holder.SubselectStrategies = startResult.SubselectStrategies;
            holder.TableAccessStrategies = startResult.TableAccessStrategies;

            if (startResult is StatementAgentInstanceFactorySelectResult selectResult) {
                holder.TopViewables = selectResult.TopViews;
                holder.EventStreamViewables = selectResult.EventStreamViewables;
                holder.PatternRoots = selectResult.PatternRoots;
                holder.AggregationService = selectResult.OptionalAggegationService;
            }
            else if (startResult is StatementAgentInstanceFactoryCreateContextResult result) {
                holder.ContextManagerRealization = result.ContextManagerRealization;
            }
            else if (startResult is StatementAgentInstanceFactoryCreateNwResult nwResult) {
                holder.TopViewables = new[] { nwResult.TopView };
                holder.NamedWindowInstance = nwResult.NamedWindowInstance;
            }
            else if (startResult is StatementAgentInstanceFactoryCreateTableResult createResult) {
                holder.TopViewables = new[] { createResult.FinalView };
                holder.TableInstance = createResult.TableInstance;
            }

            return holder;
        }
    }
} // end of namespace