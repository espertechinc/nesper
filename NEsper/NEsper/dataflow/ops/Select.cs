///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.activator;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.start;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;
using com.espertech.esper.dataflow.ops.epl;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.epl.agg.rollup;
using com.espertech.esper.epl.annotation;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.spec.util;
using com.espertech.esper.epl.view;
using com.espertech.esper.events;
using com.espertech.esper.filter;
using com.espertech.esper.util;

namespace com.espertech.esper.dataflow.ops
{
    [DataFlowOperator]
    public class Select : OutputProcessViewCallback, DataFlowOpLifecycle
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

#pragma warning disable CS0649
        [DataFlowOpParameter] private StatementSpecRaw select;
        [DataFlowOpParameter] private bool iterate;
        [DataFlowContext] private EPDataFlowEmitter graphContext;
#pragma warning restore CS0649

        private EPLSelectViewable[] _viewablesPerPort;
        private EventBeanAdapterFactory[] _adapterFactories;
        private AgentInstanceContext _agentInstanceContext;
        private EPLSelectDeliveryCallback _deliveryCallback;
        private StatementAgentInstanceFactorySelectResult _selectResult;
        private bool _isOutputLimited;
        private bool _submitEventBean;

        public DataFlowOpInitializeResult Initialize(DataFlowOpInitializateContext context)
        {
            if (context.InputPorts.IsEmpty())
            {
                throw new ArgumentException("Select operator requires at least one input stream");
            }
            if (context.OutputPorts.Count != 1)
            {
                throw new ArgumentException("Select operator requires one output stream but produces " + context.OutputPorts.Count + " streams");
            }

            DataFlowOpOutputPort portZero = context.OutputPorts[0];
            if (portZero.OptionalDeclaredType != null && !portZero.OptionalDeclaredType.IsUnderlying)
            {
                _submitEventBean = true;
            }

            // determine adapter factories for each type
            int numStreams = context.InputPorts.Count;
            _adapterFactories = new EventBeanAdapterFactory[numStreams];
            for (int i = 0; i < numStreams; i++)
            {
                EventType eventType = context.InputPorts.Get(i).TypeDesc.EventType;
                _adapterFactories[i] = context.StatementContext.EventAdapterService.GetAdapterFactoryForType(eventType);
            }

            // Compile and prepare execution
            //
            StatementContext statementContext = context.StatementContext;
            EPServicesContext servicesContext = context.ServicesContext;
            AgentInstanceContext agentInstanceContext = context.AgentInstanceContext;

            // validate
            if (select.InsertIntoDesc != null)
            {
                throw new ExprValidationException("Insert-into clause is not supported");
            }
            if (select.SelectStreamSelectorEnum != SelectClauseStreamSelectorEnum.ISTREAM_ONLY)
            {
                throw new ExprValidationException("Selecting remove-stream is not supported");
            }
            ExprNodeSubselectDeclaredDotVisitor visitor = StatementSpecRawAnalyzer.WalkSubselectAndDeclaredDotExpr(select);
            GroupByClauseExpressions groupByExpressions = GroupByExpressionHelper.GetGroupByRollupExpressions(
                servicesContext.Container,
                select.GroupByExpressions,
                select.SelectClauseSpec,
                select.HavingExprRootNode,
                select.OrderByList,
                visitor);
            if (!visitor.Subselects.IsEmpty())
            {
                throw new ExprValidationException("Subselects are not supported");
            }

            IDictionary<int, FilterStreamSpecRaw> streams = new Dictionary<int, FilterStreamSpecRaw>();
            for (int streamNum = 0; streamNum < select.StreamSpecs.Count; streamNum++)
            {
                var rawStreamSpec = select.StreamSpecs[streamNum];
                if (!(rawStreamSpec is FilterStreamSpecRaw))
                {
                    throw new ExprValidationException("From-clause must contain only streams and cannot contain patterns or other constructs");
                }
                streams.Put(streamNum, (FilterStreamSpecRaw)rawStreamSpec);
            }

            // compile offered streams
            IList<StreamSpecCompiled> streamSpecCompileds = new List<StreamSpecCompiled>();
            for (int streamNum = 0; streamNum < select.StreamSpecs.Count; streamNum++)
            {
                var filter = streams.Get(streamNum);
                var inputPort = FindInputPort(filter.RawFilterSpec.EventTypeName, context.InputPorts);
                if (inputPort == null)
                {
                    throw new ExprValidationException(
                        string.Format("Failed to find stream '{0}' among input ports, input ports are {1}", filter.RawFilterSpec.EventTypeName, GetInputPortNames(context.InputPorts).Render(", ", "[]")));
                }
                var eventType = inputPort.Value.Value.TypeDesc.EventType;
                var streamAlias = filter.OptionalStreamName;
                var filterSpecCompiled = new FilterSpecCompiled(eventType, streamAlias, new IList<FilterSpecParam>[] { Collections.GetEmptyList<FilterSpecParam>() }, null);
                var filterStreamSpecCompiled = new FilterStreamSpecCompiled(filterSpecCompiled, select.StreamSpecs[0].ViewSpecs, streamAlias, StreamSpecOptions.DEFAULT);
                streamSpecCompileds.Add(filterStreamSpecCompiled);
            }

            // create compiled statement spec
            SelectClauseSpecCompiled selectClauseCompiled = StatementLifecycleSvcUtil.CompileSelectClause(select.SelectClauseSpec);

            // determine if snapshot output is needed
            OutputLimitSpec outputLimitSpec = select.OutputLimitSpec;
            _isOutputLimited = outputLimitSpec != null;
            if (iterate)
            {
                if (outputLimitSpec != null)
                {
                    throw new ExprValidationException("Output rate limiting is not supported with 'iterate'");
                }
                outputLimitSpec = new OutputLimitSpec(OutputLimitLimitType.SNAPSHOT, OutputLimitRateType.TERM);
            }

            var mergedAnnotations = AnnotationUtil.MergeAnnotations(statementContext.Annotations, context.OperatorAnnotations);
            var orderByArray = OrderByItem.ToArray(select.OrderByList);
            var outerJoinArray = OuterJoinDesc.ToArray(select.OuterJoinDescList);
            var streamSpecArray = streamSpecCompileds.ToArray();
            var compiled = new StatementSpecCompiled(null, null, null, null, null, null, null, SelectClauseStreamSelectorEnum.ISTREAM_ONLY,
                    selectClauseCompiled, streamSpecArray, outerJoinArray, select.FilterExprRootNode, select.HavingExprRootNode, outputLimitSpec,
                    orderByArray, ExprSubselectNode.EMPTY_SUBSELECT_ARRAY, ExprNodeUtility.EMPTY_DECLARED_ARR, ExprNodeUtility.EMPTY_SCRIPTS, select.ReferencedVariables,
                    select.RowLimitSpec, CollectionUtil.EMPTY_STRING_ARRAY, mergedAnnotations, null, null, null, null, null, null, null, null, null, groupByExpressions, null, null);

            // create viewable per port
            var viewables = new EPLSelectViewable[context.InputPorts.Count];
            _viewablesPerPort = viewables;
            foreach (var entry in context.InputPorts)
            {
                EPLSelectViewable viewable = new EPLSelectViewable(entry.Value.TypeDesc.EventType);
                viewables[entry.Key] = viewable;
            }

            var activatorFactory = new ProxyViewableActivatorFactory
            {
                ProcCreateActivatorSimple = filterStreamSpec =>
                {
                    EPLSelectViewable found = null;
                    foreach (EPLSelectViewable sviewable in viewables)
                    {
                        if (sviewable.EventType == filterStreamSpec.FilterSpec.FilterForEventType)
                        {
                            found = sviewable;
                        }
                    }
                    if (found == null)
                    {
                        throw new IllegalStateException("Failed to find viewable for filter");
                    }
                    EPLSelectViewable viewable = found;
                    return new ProxyViewableActivator(
                        (agentInstanceContext2, isSubselect, isRecoveringResilient) =>
                        new ViewableActivationResult(
                            viewable,
                            new ProxyStopCallback(() => { }),
                            null,
                            null,
                            null,
                            false,
                            false,
                            null));
                }
            };

            // for per-row deliver, register select expression result callback
            OutputProcessViewCallback optionalOutputProcessViewCallback = null;
            if (!iterate && !_isOutputLimited)
            {
                _deliveryCallback = new EPLSelectDeliveryCallback();
                optionalOutputProcessViewCallback = this;
            }

            // prepare
            EPStatementStartMethodSelectDesc selectDesc = EPStatementStartMethodSelectUtil.Prepare(compiled, servicesContext, statementContext, false, agentInstanceContext, false, activatorFactory, optionalOutputProcessViewCallback, _deliveryCallback);

            // start
            _selectResult = (StatementAgentInstanceFactorySelectResult)selectDesc.StatementAgentInstanceFactorySelect.NewContext(agentInstanceContext, false);

            // for output-rate-limited, register a dispatch view
            if (_isOutputLimited)
            {
                _selectResult.FinalView.AddView(new EPLSelectUpdateDispatchView(this));
            }

            // assign strategies to expression nodes
            EPStatementStartMethodHelperAssignExpr.AssignExpressionStrategies(
                selectDesc,
                _selectResult.OptionalAggegationService,
                _selectResult.SubselectStrategies,
                _selectResult.PriorNodeStrategies,
                _selectResult.PreviousNodeStrategies,
                null,
                null,
                _selectResult.TableAccessEvalStrategies);

            EventType outputEventType = selectDesc.ResultSetProcessorPrototypeDesc.ResultSetProcessorFactory.ResultEventType;
            _agentInstanceContext = agentInstanceContext;
            return new DataFlowOpInitializeResult(new GraphTypeDesc[] { new GraphTypeDesc(false, true, outputEventType) });
        }

        private String[] GetInputPortNames(IDictionary<int, DataFlowOpInputPort> inputPorts)
        {
            IList<String> portNames = new List<String>();
            foreach (var entry in inputPorts)
            {
                if (entry.Value.OptionalAlias != null)
                {
                    portNames.Add(entry.Value.OptionalAlias);
                    continue;
                }
                if (entry.Value.StreamNames.Count == 1)
                {
                    portNames.Add(entry.Value.StreamNames.FirstOrDefault());
                }
            }
            return portNames.ToArray();
        }

        private KeyValuePair<int, DataFlowOpInputPort>? FindInputPort(String eventTypeName, IDictionary<int, DataFlowOpInputPort> inputPorts)
        {
            foreach (var entry in inputPorts)
            {
                if (entry.Value.OptionalAlias != null && entry.Value.OptionalAlias.Equals(eventTypeName))
                {
                    return entry;
                }
                if (entry.Value.StreamNames.Count == 1 && (entry.Value.StreamNames.FirstOrDefault() == eventTypeName))
                {
                    return entry;
                }
            }
            return null;
        }

        public void OnInput(int originatingStream, Object row)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("Received row from stream " + originatingStream + " for select, row is " + row);
            }

            EventBean theEvent = _adapterFactories[originatingStream].MakeAdapter(row);

            using (_agentInstanceContext.StatementContext.DefaultAgentInstanceLock.AcquireWriteLock())
            {
                try
                {
                    _viewablesPerPort[originatingStream].Process(theEvent);
                    if (_viewablesPerPort.Length > 1)
                    {
                        _agentInstanceContext.EpStatementAgentInstanceHandle.OptionalDispatchable.Execute();
                    }
                }
                finally
                {
                    if (_agentInstanceContext.StatementContext.EpStatementHandle.HasTableAccess)
                    {
                        _agentInstanceContext.StatementContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                    }
                }
            }
        }

        public void OnSignal(EPDataFlowSignal signal)
        {
            if (iterate && signal is EPDataFlowSignalFinalMarker)
            {
                IEnumerator<EventBean> it = _selectResult.FinalView.GetEnumerator();
                if (it != null)
                {
                    for (; it.MoveNext(); )
                    {
                        var @event = it.Current;
                        if (_submitEventBean)
                        {
                            graphContext.Submit(@event);
                        }
                        else
                        {
                            graphContext.Submit(@event.Underlying);
                        }
                    }
                }
            }
        }

        public void Open(DataFlowOpOpenContext openContext)
        {
            // no action
        }

        public void Close(DataFlowOpCloseContext openContext)
        {
            if (_selectResult != null)
            {
                StatementAgentInstanceUtil.StopSafe(_selectResult.StopCallback, _agentInstanceContext.StatementContext);
            }
        }

        public void OutputViaCallback(EventBean[] events)
        {
            var delivered = _deliveryCallback.Delivered;
            if (Log.IsDebugEnabled)
            {
                Log.Debug("Submitting select-output row: " + delivered.Render());
            }
            graphContext.Submit(_deliveryCallback.Delivered);
            _deliveryCallback.Reset();
        }

        public void OutputOutputRateLimited(UniformPair<EventBean[]> result)
        {
            if (result == null || result.First == null || result.First.Length == 0)
            {
                return;
            }
            foreach (EventBean item in result.First)
            {
                if (_submitEventBean)
                {
                    graphContext.Submit(item);
                }
                else
                {
                    graphContext.Submit(item.Underlying);
                }
            }
        }
    }
}
