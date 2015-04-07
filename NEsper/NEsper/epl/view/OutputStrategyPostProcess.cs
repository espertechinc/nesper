///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.collection;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.table.strategy;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.view
{
	/// <summary>
	/// An output strategy that handles routing (insert-into) and stream selection.
	/// </summary>
	public class OutputStrategyPostProcess
	{
	    private readonly OutputStrategyPostProcessFactory _parent;
	    private readonly AgentInstanceContext _agentInstanceContext;
	    private readonly TableStateInstance _tableStateInstance;
	    private readonly bool _audit;

	    public OutputStrategyPostProcess(OutputStrategyPostProcessFactory parent, AgentInstanceContext agentInstanceContext, TableStateInstance tableStateInstance)
        {
	        _parent = parent;
	        _agentInstanceContext = agentInstanceContext;
	        _tableStateInstance = tableStateInstance;
	        _audit = AuditEnum.INSERT.GetAudit(agentInstanceContext.StatementContext.Annotations) != null;
	    }

	    public void Output(bool forceUpdate, UniformPair<EventBean[]> result, UpdateDispatchView finalView)
	    {
	        var newEvents = result != null ? result.First : null;
	        var oldEvents = result != null ? result.Second : null;

	        // route first
	        if (_parent.IsRoute)
	        {
                if ((newEvents != null) && (_parent.InsertIntoStreamSelector.Value.IsSelectsIStream()))
	            {
	                Route(newEvents, _agentInstanceContext);
	            }

                if ((oldEvents != null) && (_parent.InsertIntoStreamSelector.Value.IsSelectsRStream()))
	            {
	                Route(oldEvents, _agentInstanceContext);
	            }
	        }

	        // discard one side of results
	        if (_parent.SelectStreamDirEnum == SelectClauseStreamSelectorEnum.RSTREAM_ONLY)
	        {
	            newEvents = oldEvents;
	            oldEvents = null;
	        }
	        else if (_parent.SelectStreamDirEnum == SelectClauseStreamSelectorEnum.ISTREAM_ONLY)
	        {
	            oldEvents = null;   // since the insert-into may require rstream
	        }

	        // dispatch
	        if(newEvents != null || oldEvents != null)
	        {
	            finalView.NewResult(new UniformPair<EventBean[]>(newEvents, oldEvents));
	        }
	        else if(forceUpdate)
	        {
	            finalView.NewResult(new UniformPair<EventBean[]>(null, null));
	        }
	    }

	    private void Route(EventBean[] events, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        foreach (var routed in events) {
	            if (routed is NaturalEventBean) {
	                var natural = (NaturalEventBean) routed;
	                if (_audit) {
	                    AuditPath.AuditInsertInto(_agentInstanceContext.EngineURI, _agentInstanceContext.StatementName, natural.OptionalSynthetic);
	                }
	                if (_tableStateInstance != null) {
	                    _tableStateInstance.AddEventUnadorned(natural.OptionalSynthetic);
	                }
	                else {
	                    _parent.InternalEventRouter.Route(natural.OptionalSynthetic, _parent.EpStatementHandle, _agentInstanceContext.StatementContext.InternalEventEngineRouteDest, exprEvaluatorContext, _parent.IsAddToFront);
	                }
	            }
	            else {
	                if (_audit) {
	                    AuditPath.AuditInsertInto(_agentInstanceContext.EngineURI, _agentInstanceContext.StatementName, routed);
	                }
	                if (_tableStateInstance != null) {
	                    ExprTableEvalLockUtil.ObtainLockUnless(_tableStateInstance.TableLevelRWLock.WriteLock, exprEvaluatorContext);
	                    _tableStateInstance.AddEventUnadorned(routed);
	                }
	                else {
	                    _parent.InternalEventRouter.Route(routed, _parent.EpStatementHandle, _agentInstanceContext.StatementContext.InternalEventEngineRouteDest, exprEvaluatorContext, _parent.IsAddToFront);
	                }
	            }
	        }
	    }
	}
} // end of namespace
