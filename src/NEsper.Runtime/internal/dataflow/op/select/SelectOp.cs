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
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly AgentInstanceContext agentInstanceContext;

        private readonly SelectFactory factory;
        private readonly StatementAgentInstanceFactorySelectResult startResult;

#pragma warning disable 649
        [DataFlowContext] private EPDataFlowEmitter graphContext;
#pragma warning restore 649

        public SelectOp(
            SelectFactory factory,
            AgentInstanceContext agentInstanceContext)
        {
            this.factory = factory;
            this.agentInstanceContext = agentInstanceContext;

            startResult = (StatementAgentInstanceFactorySelectResult) factory.FactorySelect.NewContext(agentInstanceContext, false);
            startResult.FinalView.Child = this;
            AIRegistryUtil.AssignFutures(
                factory.ResourceRegistry, agentInstanceContext.AgentInstanceId, startResult.OptionalAggegationService, startResult.PriorStrategies,
                startResult.PreviousGetterStrategies, startResult.SubselectStrategies, startResult.TableAccessStrategies,
                startResult.RowRecogPreviousStrategy);
        }

        public void Open(DataFlowOpOpenContext openContext)
        {
        }

        public void Close(DataFlowOpCloseContext closeContext)
        {
            AgentInstanceUtil.Stop(startResult.StopCallback, agentInstanceContext, startResult.FinalView, false, false);
            factory.ResourceRegistry.Deassign(agentInstanceContext.AgentInstanceId);
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
                if (factory.IsSubmitEventBean) {
                    graphContext.Submit(item);
                }
                else {
                    graphContext.Submit(item.Underlying);
                }
            }
        }

        public override EventType EventType => factory.FactorySelect.StatementEventType;

        public void OnInput(
            int originatingStream,
            object row)
        {
            if (log.IsDebugEnabled) {
                log.Debug("Received row from stream " + originatingStream + " for select, row is " + row);
            }

            var theEvent = factory.AdapterFactories[originatingStream].MakeAdapter(row);

            agentInstanceContext.AgentInstanceLock.AcquireWriteLock();
            try {
                var target = factory.OriginatingStreamToViewableStream[originatingStream];
                startResult.ViewableActivationResults[target].Viewable.Child.Update(new[] {theEvent}, null);
                if (startResult.ViewableActivationResults.Length > 1) {
                    agentInstanceContext.EpStatementAgentInstanceHandle.OptionalDispatchable.Execute();
                }
            }
            finally {
                if (agentInstanceContext.StatementContext.EpStatementHandle.HasTableAccess) {
                    agentInstanceContext.StatementContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                }

                agentInstanceContext.AgentInstanceLock.ReleaseWriteLock();
            }
        }

        public void OnSignal(EPDataFlowSignal signal)
        {
            if (factory.IsIterate && signal is EPDataFlowSignalFinalMarker) {
                using (var enumerator = startResult.FinalView.GetEnumerator())
                {
                    while (enumerator.MoveNext()) {
                        var @event = enumerator.Current;
                        if (factory.IsSubmitEventBean) {
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