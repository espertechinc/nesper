///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.aifactory.@select;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.statement.dispatch;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.runtime.@internal.dataflow.op.select
{
    public class SelectOp : ViewSupport,
        DataFlowOperator,
        DataFlowOperatorLifecycle,
        UpdateDispatchView
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly SelectFactory _factory;
        private readonly StatementAgentInstanceFactorySelectResult _startResult;

#pragma warning disable 649
        [DataFlowContext] private EPDataFlowEmitter graphContext;
#pragma warning restore 649

        public SelectOp(
            SelectFactory factory,
            AgentInstanceContext agentInstanceContext)
        {
            this._factory = factory;
            this._agentInstanceContext = agentInstanceContext;

            _startResult = (StatementAgentInstanceFactorySelectResult) factory.FactorySelect.NewContext(agentInstanceContext, false);
            _startResult.FinalView.Child = this;
            AIRegistryUtil.AssignFutures(
                factory.ResourceRegistry, agentInstanceContext.AgentInstanceId, _startResult.OptionalAggegationService, _startResult.PriorStrategies,
                _startResult.PreviousGetterStrategies, _startResult.SubselectStrategies, _startResult.TableAccessStrategies,
                _startResult.RowRecogPreviousStrategy);
        }

        public void Open(DataFlowOpOpenContext openContext)
        {
        }

        public void Close(DataFlowOpCloseContext closeContext)
        {
            AgentInstanceUtil.Stop(_startResult.StopCallback, _agentInstanceContext, _startResult.FinalView, false, false);
            _factory.ResourceRegistry.Deassign(_agentInstanceContext.AgentInstanceId);
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            throw new UnsupportedOperationException("Not implemented");
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return EnumerationHelper.Empty<EventBean>();
        }

        public void NewResult(UniformPair<EventBean[]> result)
        {
            if (result == null || result.First == null || result.First.Length == 0) {
                return;
            }

            foreach (var item in result.First) {
                if (_factory.IsSubmitEventBean) {
                    graphContext.Submit(item);
                }
                else {
                    graphContext.Submit(item.Underlying);
                }
            }
        }

        public override EventType EventType => _factory.FactorySelect.StatementEventType;

        public void OnInput(
            int originatingStream,
            object row)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug("Received row from stream " + originatingStream + " for select, row is " + row);
            }

            var theEvent = _factory.AdapterFactories[originatingStream].MakeAdapter(row);

            _agentInstanceContext.AgentInstanceLock.AcquireWriteLock();
            try {
                var target = _factory.OriginatingStreamToViewableStream[originatingStream];
                _startResult.ViewableActivationResults[target].Viewable.Child.Update(new[] {theEvent}, null);
                if (_startResult.ViewableActivationResults.Length > 1) {
                    _agentInstanceContext.EpStatementAgentInstanceHandle.OptionalDispatchable.Execute();
                }
            }
            finally {
                if (_agentInstanceContext.StatementContext.EpStatementHandle.HasTableAccess) {
                    _agentInstanceContext.StatementContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                }

                _agentInstanceContext.AgentInstanceLock.ReleaseWriteLock();
            }
        }

        public void OnSignal(EPDataFlowSignal signal)
        {
            if (_factory.IsIterate && signal is EPDataFlowSignalFinalMarker) {
                using (var enumerator = _startResult.FinalView.GetEnumerator())
                {
                    while (enumerator.MoveNext()) {
                        var @event = enumerator.Current;
                        if (_factory.IsSubmitEventBean) {
                            graphContext.Submit(@event);
                        }
                        else {
                            graphContext.Submit(@event.Underlying);
                        }
                    }
                }
            }
        }
    }
} // end of namespace