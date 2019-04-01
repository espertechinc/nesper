///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.statement.dispatch;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.output.view
{
	/// <summary>
	/// An output strategy that handles routing (insert-into) and stream selection.
	/// </summary>
	public class OutputStrategyPostProcess {
	    private readonly OutputStrategyPostProcessFactory parent;
	    private readonly AgentInstanceContext agentInstanceContext;
	    private readonly TableInstance tableInstance;
	    private readonly bool audit;

	    public OutputStrategyPostProcess(OutputStrategyPostProcessFactory parent, AgentInstanceContext agentInstanceContext, TableInstance tableInstance) {
	        this.parent = parent;
	        this.agentInstanceContext = agentInstanceContext;
	        this.tableInstance = tableInstance;
	        this.audit = AuditEnum.INSERT.GetAudit(agentInstanceContext.Annotations) != null;
	    }

	    public void Output(bool forceUpdate, UniformPair<EventBean[]> result, UpdateDispatchView finalView) {
	        EventBean[] newEvents = result != null ? result.First : null;
	        EventBean[] oldEvents = result != null ? result.Second : null;

	        // route first
	        if (parent.IsRoute) {
	            if ((newEvents != null) && (parent.InsertIntoStreamSelector.IsSelectsIStream)) {
	                Route(newEvents, agentInstanceContext);
	            }

	            if ((oldEvents != null) && (parent.InsertIntoStreamSelector.IsSelectsRStream)) {
	                Route(oldEvents, agentInstanceContext);
	            }
	        }

	        // discard one side of results
	        if (parent.SelectStreamDirEnum == SelectClauseStreamSelectorEnum.RSTREAM_ONLY) {
	            newEvents = oldEvents;
	            oldEvents = null;
	        } else if (parent.SelectStreamDirEnum == SelectClauseStreamSelectorEnum.ISTREAM_ONLY) {
	            oldEvents = null;   // since the insert-into may require rstream
	        }

	        // dispatch
	        if (newEvents != null || oldEvents != null) {
	            finalView.NewResult(new UniformPair<EventBean[]>(newEvents, oldEvents));
	        } else if (forceUpdate) {
	            finalView.NewResult(new UniformPair<EventBean[]>(null, null));
	        }
	    }

	    private void Route(EventBean[] events, ExprEvaluatorContext exprEvaluatorContext) {
	        foreach (EventBean routed in events) {
	            if (routed is NaturalEventBean) {
	                NaturalEventBean natural = (NaturalEventBean) routed;
	                if (audit) {
	                    agentInstanceContext.AuditProvider.Insert(natural.OptionalSynthetic, agentInstanceContext);
	                }
	                if (tableInstance != null) {
	                    TableEvalLockUtil.ObtainLockUnless(tableInstance.TableLevelRWLock.WriteLock(), exprEvaluatorContext);
	                    tableInstance.AddEventUnadorned(natural.OptionalSynthetic);
	                } else {
	                    agentInstanceContext.InternalEventRouter.Route(natural.OptionalSynthetic, agentInstanceContext, parent.IsAddToFront);
	                }
	            } else {
	                if (audit) {
	                    agentInstanceContext.AuditProvider.Insert(routed, agentInstanceContext);
	                }
	                if (tableInstance != null) {
	                    TableEvalLockUtil.ObtainLockUnless(tableInstance.TableLevelRWLock.WriteLock(), exprEvaluatorContext);
	                    tableInstance.AddEventUnadorned(routed);
	                } else {
	                    agentInstanceContext.InternalEventRouter.Route(routed, agentInstanceContext, parent.IsAddToFront);
	                }
	            }
	        }
	    }
	}
} // end of namespace