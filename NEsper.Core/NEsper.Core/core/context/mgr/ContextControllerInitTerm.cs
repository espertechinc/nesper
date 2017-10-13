///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.filter;
using com.espertech.esper.pattern;
using com.espertech.esper.schedule;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerInitTerm
        : ContextController
        , ContextControllerConditionCallback
    {
        private readonly int _pathId;
        protected readonly ContextControllerLifecycleCallback ActivationCallback;
        private readonly ContextControllerInitTermFactoryImpl _factory;

        protected ContextControllerCondition StartCondition;
        private readonly IDictionary<Object, EventBean> _distinctContexts;
        private EventBean _nonDistinctLastTrigger;
        private readonly ExprEvaluator[] _distinctEvaluators;
        private readonly EventBean[] _eventsPerStream = new EventBean[1];

        protected IDictionary<ContextControllerCondition, ContextControllerInitTermInstance> EndConditions =
            new LinkedHashMap<ContextControllerCondition, ContextControllerInitTermInstance>();

        protected int CurrentSubpathId;

        internal IDictionary<object, EventBean> DistinctContexts
        {
            get { return _distinctContexts; }
        }

        internal ExprEvaluator[] DistinctEvaluators
        {
            get { return _distinctEvaluators; }
        }

        public ContextControllerInitTerm(int pathId, ContextControllerLifecycleCallback lifecycleCallback, ContextControllerInitTermFactoryImpl factory)
        {
            _pathId = pathId;
            ActivationCallback = lifecycleCallback;
            _factory = factory;

            var contextDetail = factory.ContextDetail as ContextDetailInitiatedTerminated;
            if (contextDetail != null && contextDetail.DistinctExpressions != null && contextDetail.DistinctExpressions.Length > 0)
            {
                _distinctContexts = new Dictionary<Object, EventBean>().WithNullSupport();
                _distinctEvaluators = ExprNodeUtility.GetEvaluators(contextDetail.DistinctExpressions);
            }
        }

        public void ImportContextPartitions(ContextControllerState state, int pathIdToUse, ContextInternalFilterAddendum filterAddendum, AgentInstanceSelector agentInstanceSelector)
        {
            InitializeFromState(null, null, filterAddendum, state, pathIdToUse, agentInstanceSelector, true);
        }

        public void DeletePath(ContextPartitionIdentifier identifier)
        {
            var initterm = (ContextPartitionIdentifierInitiatedTerminated)identifier;
            foreach (var entry in EndConditions)
            {
                if (Compare(initterm.StartTime, initterm.Properties, initterm.EndTime,
                        entry.Value.StartTime, entry.Value.StartProperties, entry.Value.EndTime))
                {
                    entry.Key.Deactivate();
                    EndConditions.Remove(entry.Key);
                    RemoveDistinctKey(entry.Value);
                    break;
                }
            }
        }

        public void Activate(EventBean optionalTriggeringEvent, IDictionary<String, Object> optionalTriggeringPattern, ContextControllerState controllerState, ContextInternalFilterAddendum filterAddendum, int? importPathId)
        {
            if (_factory.FactoryContext.NestingLevel == 1)
            {
                controllerState = ContextControllerStateUtil.GetRecoveryStates(_factory.FactoryContext.StateCache, _factory.FactoryContext.OutermostContextName);
            }

            bool currentlyRunning;
            var contextDetailInitiatedTerminated = _factory.ContextDetailInitiatedTerminated;
            if (controllerState == null)
            {
                StartCondition = MakeEndpoint(contextDetailInitiatedTerminated.Start, filterAddendum, true, 0);

                // if this is single-instance mode, check if we are currently running according to schedule
                currentlyRunning = StartCondition.IsImmediate;
                if (!contextDetailInitiatedTerminated.IsOverlapping)
                {
                    currentlyRunning = DetermineCurrentlyRunning(StartCondition);
                }

                if (currentlyRunning)
                {
                    CurrentSubpathId++;
                    var endEndpoint = MakeEndpoint(contextDetailInitiatedTerminated.End, filterAddendum, false, CurrentSubpathId);
                    endEndpoint.Activate(optionalTriggeringEvent, null, 0, _factory.FactoryContext.IsRecoveringResilient);
                    var startTime = _factory.SchedulingService.Time;
                    var endTime = endEndpoint.ExpectedEndTime;
                    var builtinProps = GetBuiltinProperties(_factory.FactoryContext.ContextName, startTime, endTime, Collections.GetEmptyMap<string, object>());
                    var instanceHandle = ActivationCallback.ContextPartitionInstantiate(null, CurrentSubpathId, null, this, optionalTriggeringEvent, optionalTriggeringPattern, null, builtinProps, controllerState, filterAddendum, _factory.FactoryContext.IsRecoveringResilient, ContextPartitionState.STARTED);
                    EndConditions.Put(endEndpoint, new ContextControllerInitTermInstance(instanceHandle, null, startTime, endTime, CurrentSubpathId));

                    var state = new ContextControllerInitTermState(_factory.FactoryContext.ServicesContext.SchedulingService.Time, builtinProps);
                    _factory.FactoryContext.StateCache.AddContextPath(_factory.FactoryContext.OutermostContextName, _factory.FactoryContext.NestingLevel, _pathId, CurrentSubpathId, instanceHandle.ContextPartitionOrPathId, state, _factory.Binding);
                }

                // non-overlapping and not currently running, or overlapping
                if ((!contextDetailInitiatedTerminated.IsOverlapping && !currentlyRunning) ||
                    contextDetailInitiatedTerminated.IsOverlapping)
                {
                    StartCondition.Activate(optionalTriggeringEvent, null, 0, _factory.FactoryContext.IsRecoveringResilient);
                }
                return;
            }

            StartCondition = MakeEndpoint(contextDetailInitiatedTerminated.Start, filterAddendum, true, 0);

            // if this is single-instance mode, check if we are currently running according to schedule
            currentlyRunning = false;
            if (!contextDetailInitiatedTerminated.IsOverlapping)
            {
                currentlyRunning = DetermineCurrentlyRunning(StartCondition);
            }
            if (!currentlyRunning)
            {
                StartCondition.Activate(optionalTriggeringEvent, null, 0, _factory.FactoryContext.IsRecoveringResilient);
            }

            int pathIdToUse = importPathId ?? _pathId;
            InitializeFromState(optionalTriggeringEvent, optionalTriggeringPattern, filterAddendum, controllerState, pathIdToUse, null, false);
        }

        protected ContextControllerCondition MakeEndpoint(ContextDetailCondition endpoint, ContextInternalFilterAddendum filterAddendum, bool isStartEndpoint, int subPathId)
        {
            return ContextControllerConditionFactory.GetEndpoint(_factory.FactoryContext.ContextName, _factory.FactoryContext.ServicesContext, _factory.FactoryContext.AgentInstanceContextCreate,
                    endpoint, this, filterAddendum, isStartEndpoint,
                    _factory.FactoryContext.NestingLevel, _pathId, subPathId);
        }

        public void VisitSelectedPartitions(ContextPartitionSelector contextPartitionSelector, ContextPartitionVisitor visitor)
        {
            var nestingLevel = _factory.FactoryContext.NestingLevel;
            if (contextPartitionSelector is ContextPartitionSelectorFiltered)
            {
                var filter = (ContextPartitionSelectorFiltered)contextPartitionSelector;
                var identifier = new ContextPartitionIdentifierInitiatedTerminated();
                foreach (var entry in EndConditions)
                {
                    identifier.EndTime = entry.Value.EndTime;
                    identifier.StartTime = entry.Value.StartTime;
                    identifier.Properties = entry.Value.StartProperties;
                    identifier.ContextPartitionId = entry.Value.InstanceHandle.ContextPartitionOrPathId;
                    if (filter.Filter(identifier))
                    {
                        var state = new ContextControllerInitTermState(_factory.FactoryContext.ServicesContext.SchedulingService.Time, entry.Value.StartProperties);
                        visitor.Visit(nestingLevel, _pathId, _factory.Binding, state, this, entry.Value.InstanceHandle);
                    }
                }
                return;
            }
            if (contextPartitionSelector is ContextPartitionSelectorById)
            {
                var filter = (ContextPartitionSelectorById)contextPartitionSelector;
                foreach (var entry in EndConditions)
                {
                    if (filter.ContextPartitionIds.Contains(entry.Value.InstanceHandle.ContextPartitionOrPathId))
                    {
                        var state = new ContextControllerInitTermState(_factory.FactoryContext.ServicesContext.SchedulingService.Time, entry.Value.StartProperties);
                        visitor.Visit(nestingLevel, _pathId, _factory.Binding, state, this, entry.Value.InstanceHandle);
                    }
                }
                return;
            }
            if (contextPartitionSelector is ContextPartitionSelectorAll)
            {
                foreach (var entry in EndConditions)
                {
                    var state = new ContextControllerInitTermState(_factory.FactoryContext.ServicesContext.SchedulingService.Time, entry.Value.StartProperties);
                    visitor.Visit(nestingLevel, _pathId, _factory.Binding, state, this, entry.Value.InstanceHandle);
                }
                return;
            }
            throw ContextControllerSelectorUtil.GetInvalidSelector(new Type[0], contextPartitionSelector);
        }

        public void RangeNotification(
            IDictionary<String, Object> builtinProperties,
            ContextControllerCondition originCondition,
            EventBean optionalTriggeringEvent,
            IDictionary<String, Object> optionalTriggeringPattern,
            ContextInternalFilterAddendum filterAddendum)
        {
            var endConditionNotification = originCondition != StartCondition;
            var startNow = StartCondition is ContextControllerConditionImmediate;
            IList<AgentInstance> agentInstancesLocksHeld = null;

            _nonDistinctLastTrigger = optionalTriggeringEvent;

            ILockable tempLock = startNow
                ? _factory.FactoryContext.ServicesContext.FilterService.WriteLock
                : new VoidLock();

            using (tempLock.Acquire())
            {
                try
                {
                    if (endConditionNotification)
                    {

                        if (originCondition.IsRunning)
                        {
                            originCondition.Deactivate();
                        }

                        // indicate terminate
                        var instance = EndConditions.Delete(originCondition);
                        if (instance == null)
                        {
                            return;
                        }

                        // For start-now (non-overlapping only) we hold the lock of the existing agent instance
                        // until the new one is ready.
                        if (startNow)
                        {
                            agentInstancesLocksHeld = new List<AgentInstance>();
                            optionalTriggeringEvent = null;
                            // since we are restarting, we don't want to evaluate the event twice
                            optionalTriggeringPattern = null;
                        }
                        ActivationCallback.ContextPartitionTerminate(
                            instance.InstanceHandle, builtinProperties, startNow, agentInstancesLocksHeld);

                        // remove distinct key
                        RemoveDistinctKey(instance);

                        // re-activate start condition if not overlapping
                        if (!_factory.ContextDetailInitiatedTerminated.IsOverlapping)
                        {
                            StartCondition.Activate(optionalTriggeringEvent, null, 0, false);
                        }

                        _factory.FactoryContext.StateCache.RemoveContextPath(
                            _factory.FactoryContext.OutermostContextName, _factory.FactoryContext.NestingLevel, _pathId,
                            instance.SubPathId);
                    }

                    // handle start-condition notification
                    if (!endConditionNotification || startNow)
                    {

                        // Check if this is distinct-only and the key already exists
                        if (_distinctContexts != null)
                        {
                            var added = AddDistinctKey(optionalTriggeringEvent);
                            if (!added)
                            {
                                return;
                            }
                        }

                        // For single-instance mode, deactivate
                        if (!_factory.ContextDetailInitiatedTerminated.IsOverlapping)
                        {
                            if (StartCondition.IsRunning)
                            {
                                StartCondition.Deactivate();
                            }
                        }
                        // For overlapping mode, make sure we activate again or stay activated
                        else
                        {
                            if (!StartCondition.IsRunning)
                            {
                                StartCondition.Activate(null, null, 0, _factory.FactoryContext.IsRecoveringResilient);
                            }
                        }

                        CurrentSubpathId++;
                        var endEndpoint = MakeEndpoint(
                            _factory.ContextDetailInitiatedTerminated.End, filterAddendum, false, CurrentSubpathId);
                        var matchedEventMap = GetMatchedEventMap(builtinProperties);
                        endEndpoint.Activate(null, matchedEventMap, 0, false);
                        var startTime = _factory.SchedulingService.Time;
                        var endTime = endEndpoint.ExpectedEndTime;
                        var builtinProps = GetBuiltinProperties(_factory.FactoryContext.ContextName, startTime, endTime, builtinProperties);
                        var instanceHandle = ActivationCallback.ContextPartitionInstantiate(
                            null, CurrentSubpathId, null, this, optionalTriggeringEvent, optionalTriggeringPattern,
                            new ContextControllerInitTermState(
                                _factory.SchedulingService.Time, matchedEventMap.MatchingEventsAsMap), builtinProps,
                            null, filterAddendum, _factory.FactoryContext.IsRecoveringResilient,
                            ContextPartitionState.STARTED);
                        EndConditions.Put(
                            endEndpoint,
                            new ContextControllerInitTermInstance(
                                instanceHandle, builtinProperties, startTime, endTime, CurrentSubpathId));

                        // install filter fault handlers, if necessary
                        InstallFilterFaultHandler(instanceHandle);

                        var state =
                            new ContextControllerInitTermState(
                                _factory.FactoryContext.ServicesContext.SchedulingService.Time, builtinProperties);
                        _factory.FactoryContext.StateCache.AddContextPath(
                            _factory.FactoryContext.OutermostContextName, _factory.FactoryContext.NestingLevel, _pathId,
                            CurrentSubpathId, instanceHandle.ContextPartitionOrPathId, state, _factory.Binding);
                    }
                }
                finally
                {
                    if (agentInstancesLocksHeld != null)
                    {
                        foreach (var agentInstance in agentInstancesLocksHeld)
                        {
                            agentInstance.AgentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion = long.MaxValue;
                            if (agentInstance.AgentInstanceContext.StatementContext.EpStatementHandle.HasTableAccess)
                            {
                                agentInstance.AgentInstanceContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                            }
                            agentInstance.AgentInstanceContext.AgentInstanceLock.WriteLock.Release();
                        }
                    }
                }
            }
        }

        private void InstallFilterFaultHandler(ContextControllerInstanceHandle instanceHandle)
        {
            FilterFaultHandler myFaultHandler = null;
            if (DistinctContexts != null)
            {
                myFaultHandler = new DistinctFilterFaultHandler(this);
            }
            else
            {
                if (StartCondition is ContextControllerConditionFilter)
                {
                    myFaultHandler = new NonDistinctFilterFaultHandler(this);
                }
            }

            if (myFaultHandler != null && instanceHandle.Instances != null)
            {
                foreach (AgentInstance agentInstance in instanceHandle.Instances.AgentInstances)
                {
                    agentInstance.AgentInstanceContext.EpStatementAgentInstanceHandle.FilterFaultHandler = myFaultHandler;
                }
            }
        }

        protected MatchedEventMap GetMatchedEventMap(IDictionary<String, Object> builtinProperties)
        {
            var props = new Object[_factory.MatchedEventMapMeta.TagsPerIndex.Length];
            var count = 0;
            foreach (var name in _factory.MatchedEventMapMeta.TagsPerIndex)
            {
                props[count++] = builtinProperties.Get(name);
            }
            return new MatchedEventMapImpl(_factory.MatchedEventMapMeta, props);
        }

        protected bool DetermineCurrentlyRunning(ContextControllerCondition startCondition)
        {
            // we are not currently running if either of the endpoints is not crontab-triggered
            var contextDetailInitiatedTerminated = _factory.ContextDetailInitiatedTerminated;
            if ((contextDetailInitiatedTerminated.Start is ContextDetailConditionCrontab) &&
               ((contextDetailInitiatedTerminated.End is ContextDetailConditionCrontab)))
            {
                var scheduleStart = ((ContextDetailConditionCrontab)contextDetailInitiatedTerminated.Start).Schedule;
                var scheduleEnd = ((ContextDetailConditionCrontab)contextDetailInitiatedTerminated.End).Schedule;

                var engineImportService = _factory.StatementContext.EngineImportService;
                var nextScheduledStartTime = ScheduleComputeHelper.ComputeNextOccurance(
                    scheduleStart, _factory.TimeProvider.Time, engineImportService.TimeZone,
                    engineImportService.TimeAbacus);
                long nextScheduledEndTime = ScheduleComputeHelper.ComputeNextOccurance(
                    scheduleEnd, _factory.TimeProvider.Time, engineImportService.TimeZone, 
                    engineImportService.TimeAbacus);

                return nextScheduledStartTime >= nextScheduledEndTime;
            }

            if (startCondition is ContextControllerConditionTimePeriod)
            {
                var condition = (ContextControllerConditionTimePeriod)startCondition;
                var endTime = condition.ExpectedEndTime;
                if (endTime != null && endTime <= 0)
                {
                    return true;
                }
            }

            return startCondition is ContextControllerConditionImmediate;
        }

        public ContextControllerFactory Factory
        {
            get { return _factory; }
        }

        public int PathId
        {
            get { return _pathId; }
        }

        public void Deactivate()
        {
            if (StartCondition != null)
            {
                if (StartCondition.IsRunning)
                {
                    StartCondition.Deactivate();
                }
            }

            foreach (var entry in EndConditions)
            {
                if (entry.Key.IsRunning)
                {
                    entry.Key.Deactivate();
                }
            }
            EndConditions.Clear();
            _factory.FactoryContext.StateCache.RemoveContextParentPath(_factory.FactoryContext.OutermostContextName, _factory.FactoryContext.NestingLevel, _pathId);
        }

        internal static IDictionary<String, Object> GetBuiltinProperties(String contextName, long startTime, long? endTime, IDictionary<String, Object> startEndpointData)
        {
            IDictionary<String, Object> props = new Dictionary<String, Object>();
            props.Put(ContextPropertyEventType.PROP_CTX_NAME, contextName);
            props.Put(ContextPropertyEventType.PROP_CTX_STARTTIME, startTime);
            props.Put(ContextPropertyEventType.PROP_CTX_ENDTIME, endTime);
            props.PutAll(startEndpointData);
            return props;
        }

        private void InitializeFromState(
            EventBean optionalTriggeringEvent,
            IDictionary<String, Object> optionalTriggeringPattern,
            ContextInternalFilterAddendum filterAddendum,
            ContextControllerState controllerState,
            int pathIdToUse,
            AgentInstanceSelector agentInstanceSelector,
            bool loadingExistingState)
        {
            var states = controllerState.States;
            var childContexts = ContextControllerStateUtil.GetChildContexts(_factory.FactoryContext, pathIdToUse, states);
            var eventAdapterService = _factory.FactoryContext.ServicesContext.EventAdapterService;

            var maxSubpathId = int.MinValue;
            foreach (var entry in childContexts)
            {
                var state = (ContextControllerInitTermState)_factory.Binding.ByteArrayToObject(entry.Value.Blob, eventAdapterService);

                if (_distinctContexts != null)
                {
                    var filter = (ContextControllerConditionFilter)StartCondition;
                    var @event = (EventBean)state.PatternData.Get(filter.EndpointFilterSpec.OptionalFilterAsName);
                    AddDistinctKey(@event);
                }

                if (controllerState.IsImported)
                {
                    KeyValuePair<ContextControllerCondition, ContextControllerInitTermInstance>? existing = null;
                    foreach (var entryExisting in EndConditions)
                    {
                        if (Compare(state.StartTime, state.PatternData, null,
                                    entryExisting.Value.StartTime, entryExisting.Value.StartProperties, null))
                        {
                            existing = entryExisting;
                            break;
                        }
                    }
                    if (existing != null)
                    {
                        ContextControllerInstanceHandle existingHandle = existing.Value.Value.InstanceHandle;
                        if (existingHandle != null)
                        {
                            ActivationCallback.ContextPartitionNavigate(existingHandle, this, controllerState, entry.Value.OptionalContextPartitionId.Value, filterAddendum, agentInstanceSelector, entry.Value.Blob, loadingExistingState);
                            continue;
                        }
                    }
                }

                var endEndpoint = MakeEndpoint(_factory.ContextDetailInitiatedTerminated.End, filterAddendum, false, entry.Key.SubPath);
                var timeOffset = _factory.FactoryContext.ServicesContext.SchedulingService.Time - state.StartTime;

                endEndpoint.Activate(optionalTriggeringEvent, null, timeOffset, _factory.FactoryContext.IsRecoveringResilient);
                var startTime = state.StartTime;
                var endTime = endEndpoint.ExpectedEndTime;
                var builtinProps = GetBuiltinProperties(_factory.FactoryContext.ContextName, startTime, endTime, state.PatternData);
                var contextPartitionId = entry.Value.OptionalContextPartitionId;

                var assignedSubPathId = !controllerState.IsImported ? entry.Key.SubPath : ++CurrentSubpathId;
                var instanceHandle = ActivationCallback.ContextPartitionInstantiate(contextPartitionId, assignedSubPathId, entry.Key.SubPath, this, optionalTriggeringEvent, optionalTriggeringPattern, null, builtinProps, controllerState, filterAddendum, loadingExistingState || _factory.FactoryContext.IsRecoveringResilient, entry.Value.State);
                EndConditions.Put(endEndpoint, new ContextControllerInitTermInstance(instanceHandle, state.PatternData, startTime, endTime, assignedSubPathId));

                if (entry.Key.SubPath > maxSubpathId)
                {
                    maxSubpathId = assignedSubPathId;
                }
            }

            if (!controllerState.IsImported)
            {
                CurrentSubpathId = maxSubpathId != int.MinValue ? maxSubpathId : 0;
            }
        }

        public static bool Compare(long savedStartTime,
                                      IDictionary<String, Object> savedProperties,
                                      long? savedEndTime,
                                      long existingStartTime,
                                      IDictionary<String, Object> existingProperties,
                                      long? existingEndTime)
        {
            if (savedStartTime != existingStartTime)
            {
                return false;
            }
            if (savedEndTime != null && existingEndTime != null && !savedEndTime.Equals(existingEndTime))
            {
                return false;
            }

            foreach (var savedEntry in savedProperties)
            {
                var existingValue = existingProperties.Get(savedEntry.Key);
                var savedValue = savedEntry.Value;
                if (savedValue == null && existingValue == null)
                {
                    continue;
                }
                if (savedValue == null || existingValue == null)
                {
                    return false;
                }
                if (existingValue.Equals(savedValue))
                {
                    continue;
                }
                if (existingValue is EventBean && savedValue is EventBean)
                {
                    if (((EventBean)existingValue).Underlying.Equals(((EventBean)savedValue).Underlying))
                    {
                        continue;
                    }
                }
                return false;
            }
            return true;
        }

        private bool AddDistinctKey(EventBean optionalTriggeringEvent)
        {
            var key = GetDistinctKey(optionalTriggeringEvent);
            if (_distinctContexts.ContainsKey(key))
            {
                return false;
            }
            _distinctContexts.Put(key, optionalTriggeringEvent);
            return true;
        }

        private void RemoveDistinctKey(ContextControllerInitTermInstance value)
        {
            if (_distinctContexts == null)
            {
                return;
            }
            var filter = (ContextControllerConditionFilter)StartCondition;
            var @event = (EventBean)value.StartProperties.Get(filter.EndpointFilterSpec.OptionalFilterAsName);
            var key = GetDistinctKey(@event);
            _distinctContexts.Remove(key);
        }

        private Object GetDistinctKey(EventBean optionalTriggeringEvent)
        {
            _eventsPerStream[0] = optionalTriggeringEvent;
            if (_distinctEvaluators.Length == 1)
            {
                return _distinctEvaluators[0].Evaluate(new EvaluateParams(_eventsPerStream, true, _factory.FactoryContext.AgentInstanceContextCreate));
            }

            var results = new Object[_distinctEvaluators.Length];
            var count = 0;
            foreach (var expr in _distinctEvaluators)
            {
                results[count] = expr.Evaluate(new EvaluateParams(_eventsPerStream, true, _factory.FactoryContext.AgentInstanceContextCreate));
                count++;
            }
            return new MultiKeyUntyped(results);
        }

        internal class DistinctFilterFaultHandler : FilterFaultHandler
        {
            private readonly ContextControllerInitTerm _contextControllerInitTerm;

            internal DistinctFilterFaultHandler(ContextControllerInitTerm contextControllerInitTerm)
            {
                _contextControllerInitTerm = contextControllerInitTerm;
            }

            public bool HandleFilterFault(EventBean theEvent, long version)
            {
                /*
                 * Handle filter faults such as
                 *   - a) App thread determines event E1 applies to CTX + CP1
                 *     b) Timer thread destroys CP1
                 *     c) App thread processes E1 for CTX allocating CP2, processing E1 for CP2
                 *     d) App thread processes E1 for CP1, filter-faulting and ending up dropping the event for CP1 because of this handler
                 *
                 *   - a) App thread determines event E1 applies to CTX + CP1
                 *     b) App thread processes E1 for CTX, no action
                 *     c) Timer thread destroys CP1
                 *     d) App thread processes E1 for CP1, filter-faulting and ending up processing E1 into CTX because of this handler
                 */

                AgentInstanceContext aiCreate = _contextControllerInitTerm.Factory.FactoryContext.AgentInstanceContextCreate;
                using (aiCreate.EpStatementAgentInstanceHandle.StatementAgentInstanceLock.AcquireWriteLock())
                {
                    Object key = _contextControllerInitTerm.GetDistinctKey(theEvent);
                    EventBean trigger = _contextControllerInitTerm.DistinctContexts.Get(key);

                    // see if we find that context partition
                    if (trigger != null)
                    {
                        // true for we have already handled this event
                        // false for filter fault
                        return trigger.Equals(theEvent);
                    }

                    // not found: evaluate against context
                    StatementAgentInstanceUtil.EvaluateEventForStatement(
                        _contextControllerInitTerm.Factory.FactoryContext.ServicesContext,
                        theEvent, null, Collections.SingletonList(new AgentInstance(null, aiCreate, null)));

                    return true; // we handled the event
                }
            }
        }

        internal class NonDistinctFilterFaultHandler : FilterFaultHandler
        {
            private readonly ContextControllerInitTerm _contextControllerInitTerm;

            internal NonDistinctFilterFaultHandler(ContextControllerInitTerm contextControllerInitTerm)
            {
                _contextControllerInitTerm = contextControllerInitTerm;
            }

            public bool HandleFilterFault(EventBean theEvent, long version)
            {
                //
                // Handle filter faults such as
                //   - a) App thread determines event E1 applies to CP1
                //     b) Timer thread destroys CP1
                //     c) App thread processes E1 for CP1, filter-faulting and ending up reprocessing the event against CTX because of this handler
                //

                AgentInstanceContext aiCreate = _contextControllerInitTerm.Factory.FactoryContext.AgentInstanceContextCreate;
                using (aiCreate.EpStatementAgentInstanceHandle.StatementAgentInstanceLock.AcquireWriteLock())
                {
                    EventBean trigger = _contextControllerInitTerm._nonDistinctLastTrigger;
                    if (theEvent != trigger)
                    {
                        StatementAgentInstanceUtil.EvaluateEventForStatement(
                            _contextControllerInitTerm.Factory.FactoryContext.ServicesContext,
                            theEvent, null, Collections.SingletonList(new AgentInstance(null, aiCreate, null)));
                    }

                    return true; // we handled the event
                }
            }
        }
    }
}
