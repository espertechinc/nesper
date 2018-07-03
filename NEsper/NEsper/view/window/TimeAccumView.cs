///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// A data window view that holds events in a stream and only removes events from a stream (rstream) if
    /// no more events arrive for a given time interval.
    /// <para>
    /// No batch version of the view exists as the batch version is simply the remove stream of this view, which removes
    /// in batches.
    /// </para>
    /// <para>
    /// The view is continuous, the insert stream consists of arriving events. The remove stream
    /// only posts current window contents when no more events arrive for a given timer interval.
    /// </para>
    /// </summary>
    public class TimeAccumView
        : ViewSupport
        , CloneableView
        , DataWindowView
        , StoppableView
        , StopCallback
    {
        private static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly AgentInstanceViewFactoryChainContext _agentInstanceContext;
        private readonly ExprTimePeriodEvalDeltaConst _timeDeltaComputation;
        private readonly ViewUpdatedCollection _viewUpdatedCollection;
        private readonly long _scheduleSlot;
        // View parameters
        private readonly TimeAccumViewFactory _factory;
        // Current running parameters
        private readonly List<EventBean> _currentBatch = new List<EventBean>();
        private long _callbackScheduledTime;
        private readonly EPStatementHandleCallback _handle;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="viewUpdatedCollection">is a collection that the view must update when receiving events</param>
        /// <param name="timeBatchViewFactory">fr copying this view in a group-by</param>
        /// <param name="agentInstanceContext">is required view services</param>
        /// <param name="timeDeltaComputation">delta computation</param>
        public TimeAccumView(
            TimeAccumViewFactory timeBatchViewFactory,
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            ExprTimePeriodEvalDeltaConst timeDeltaComputation,
            ViewUpdatedCollection viewUpdatedCollection)
        {
            _agentInstanceContext = agentInstanceContext;
            _factory = timeBatchViewFactory;
            _timeDeltaComputation = timeDeltaComputation;
            _viewUpdatedCollection = viewUpdatedCollection;

            _scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();

            var callback = new ProxyScheduleHandleCallback
            {
                ProcScheduledTrigger = extensionServicesContext =>
                {
                    if (InstrumentationHelper.ENABLED)
                    {
                        InstrumentationHelper.Get().QViewScheduledEval(this, _factory.ViewName);
                    }
                    SendRemoveStream();
                    if (InstrumentationHelper.ENABLED)
                    {
                        InstrumentationHelper.Get().AViewScheduledEval();
                    }
                }
            };
            _handle = new EPStatementHandleCallback(agentInstanceContext.EpStatementAgentInstanceHandle, callback);
            agentInstanceContext.AddTerminationCallback(this);
        }

        public View CloneView()
        {
            return _factory.MakeView(_agentInstanceContext);
        }

        public ExprTimePeriodEvalDeltaConst TimeDeltaComputation => _timeDeltaComputation;

        public override EventType EventType => Parent.EventType;

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QViewProcessIRStream(this, _factory.ViewName, newData, oldData);
            }

            // we don't care about removed data from a prior view
            if ((newData == null) || (newData.Length == 0))
            {
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().AViewProcessIRStream();
                }
                return;
            }

            // If we have an empty window about to be filled for the first time, addSchedule a callback
            bool removeSchedule = false;
            bool addSchedule = false;
            long timestamp = _agentInstanceContext.StatementContext.SchedulingService.Time;

            if (!_currentBatch.IsEmpty())
            {
                // check if we need to reschedule
                long callbackTime = timestamp + _timeDeltaComputation.DeltaAdd(timestamp);
                if (callbackTime != _callbackScheduledTime)
                {
                    removeSchedule = true;
                    addSchedule = true;
                }
            }
            else
            {
                addSchedule = true;
            }

            if (removeSchedule)
            {
                _agentInstanceContext.StatementContext.SchedulingService.Remove(_handle, _scheduleSlot);
            }
            if (addSchedule)
            {
                long timeIntervalSize = _timeDeltaComputation.DeltaAdd(timestamp);
                _agentInstanceContext.StatementContext.SchedulingService.Add(timeIntervalSize, _handle, _scheduleSlot);
                _callbackScheduledTime = timeIntervalSize + timestamp;
            }

            // add data points to the window
            foreach (EventBean newEvent in newData)
            {
                _currentBatch.Add(newEvent);
            }

            // forward insert stream to child views
            if (_viewUpdatedCollection != null)
            {
                _viewUpdatedCollection.Update(newData, null);
            }

            // update child views
            if (HasViews)
            {
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().QViewIndicate(this, _factory.ViewName, newData, null);
                }
                UpdateChildren(newData, null);
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().AViewIndicate();
                }
            }

            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().AViewProcessIRStream();
            }
        }

        /// <summary>
        /// This method sends the remove stream for all accumulated events.
        /// </summary>
        protected void SendRemoveStream()
        {
            _callbackScheduledTime = -1;

            // If there are child views and the batch was filled, fireStatementStopped update method
            if (HasViews)
            {
                // Convert to object arrays
                EventBean[] oldData = null;
                if (!_currentBatch.IsEmpty())
                {
                    oldData = _currentBatch.ToArray();
                }

                // Post old data
                if (_viewUpdatedCollection != null)
                {
                    _viewUpdatedCollection.Update(null, oldData);
                }

                if (oldData != null)
                {
                    if (InstrumentationHelper.ENABLED)
                    {
                        InstrumentationHelper.Get().QViewIndicate(this, _factory.ViewName, null, oldData);
                    }
                    UpdateChildren(null, oldData);
                    if (InstrumentationHelper.ENABLED)
                    {
                        InstrumentationHelper.Get().AViewIndicate();
                    }
                }
            }

            _currentBatch.Clear();
        }

        /// <summary>
        /// Returns true if the window is empty, or false if not empty.
        /// </summary>
        /// <value>true if empty</value>
        public bool IsEmpty => _currentBatch.IsEmpty();

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _currentBatch.GetEnumerator();
        }

        public override String ToString()
        {
            return GetType().FullName;
        }

        public void StopView()
        {
            StopSchedule();
            _agentInstanceContext.RemoveTerminationCallback(this);
        }

        public void Stop()
        {
            StopSchedule();
        }

        public void StopSchedule()
        {
            if (_handle != null)
            {
                _agentInstanceContext.StatementContext.SchedulingService.Remove(_handle, _scheduleSlot);
            }
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(_currentBatch, true, _factory.ViewName, null);
        }

        public ViewFactory ViewFactory => _factory;
    }
} // end of namespace
