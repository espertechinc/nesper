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
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// Same as the <seealso cref="TimeBatchView" />, this view also supports fast-remove from the batch for remove stream events.
    /// </summary>
    public class TimeBatchViewRStream
        : ViewSupport
        , CloneableView
        , StoppableView
        , StopCallback
        , DataWindowView
    {
        private readonly AgentInstanceViewFactoryChainContext _agentInstanceContext;
        private readonly ExprTimePeriodEvalDeltaConst _timeDeltaComputation;
        private readonly long _scheduleSlot;
        // View parameters
        private readonly TimeBatchViewFactory _timeBatchViewFactory;
        private readonly long? _initialReferencePoint;
        private readonly bool _isForceOutput;
        private readonly bool _isStartEager;
        private EPStatementHandleCallback _handle;

        // Current running parameters
        private long? _currentReferencePoint;
        private LinkedHashSet<EventBean> _lastBatch = null;
        private LinkedHashSet<EventBean> _currentBatch = new LinkedHashSet<EventBean>();
        private bool _isCallbackScheduled;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="timeDeltaComputation">is the number of milliseconds to batch events for</param>
        /// <param name="referencePoint">
        /// is the reference point onto which to base intervals, or null if
        /// there is no such reference point supplied
        /// </param>
        /// <param name="timeBatchViewFactory">fr copying this view in a group-by</param>
        /// <param name="forceOutput">is true if the batch should produce empty output if there is no value to output following time intervals</param>
        /// <param name="isStartEager">is true for start-eager</param>
        /// <param name="agentInstanceContext">context</param>
        public TimeBatchViewRStream(
            TimeBatchViewFactory timeBatchViewFactory,
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            ExprTimePeriodEvalDeltaConst timeDeltaComputation,
            long? referencePoint,
            bool forceOutput,
            bool isStartEager)
        {
            _agentInstanceContext = agentInstanceContext;
            _timeBatchViewFactory = timeBatchViewFactory;
            _timeDeltaComputation = timeDeltaComputation;
            _initialReferencePoint = referencePoint;
            _isStartEager = isStartEager;
            _isForceOutput = forceOutput;

            _scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();

            // schedule the first callback
            if (_isStartEager)
            {
                _currentReferencePoint = agentInstanceContext.StatementContext.SchedulingService.Time;
                ScheduleCallback();
                _isCallbackScheduled = true;
            }
            agentInstanceContext.AddTerminationCallback(this);
        }

        public View CloneView()
        {
            return _timeBatchViewFactory.MakeView(_agentInstanceContext);
        }

        public ExprTimePeriodEvalDeltaConst TimeDeltaComputation => _timeDeltaComputation;

        /// <summary>
        /// Gets the reference point to use to anchor interval start and end dates to.
        /// </summary>
        /// <value>is the millisecond reference point.</value>
        public long? InitialReferencePoint => _initialReferencePoint;

        /// <summary>
        /// True for force-output.
        /// </summary>
        /// <value>indicates force-output</value>
        public bool IsForceOutput => _isForceOutput;

        /// <summary>
        /// True for start-eager.
        /// </summary>
        /// <value>indicates start-eager</value>
        public bool IsStartEager => _isStartEager;

        public override EventType EventType => Parent.EventType;

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QViewProcessIRStream(this, _timeBatchViewFactory.ViewName, newData, oldData);
            }

            if (oldData != null)
            {
                for (int i = 0; i < oldData.Length; i++)
                {
                    _currentBatch.Remove(oldData[i]);
                    InternalHandleRemoved(oldData[i]);
                }
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

            // If we have an empty window about to be filled for the first time, schedule a callback
            if (_currentBatch.IsEmpty())
            {
                if (_currentReferencePoint == null)
                {
                    _currentReferencePoint = _initialReferencePoint;
                    if (_currentReferencePoint == null)
                    {
                        _currentReferencePoint = _agentInstanceContext.StatementContext.SchedulingService.Time;
                    }
                }

                // Schedule the next callback if there is none currently scheduled
                if (!_isCallbackScheduled)
                {
                    ScheduleCallback();
                    _isCallbackScheduled = true;
                }
            }

            // add data points to the timeWindow
            foreach (EventBean newEvent in newData)
            {
                _currentBatch.Add(newEvent);
            }

            // We do not update child views, since we batch the events.
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().AViewProcessIRStream();
            }
        }

        /// <summary>
        /// This method updates child views and clears the batch of events.
        /// We schedule a new callback at this time if there were events in the batch.
        /// </summary>
        protected void SendBatch()
        {
            _isCallbackScheduled = false;

            // If there are child views and the batch was filled, fireStatementStopped update method
            if (HasViews)
            {
                // Convert to object arrays
                EventBean[] newData = null;
                EventBean[] oldData = null;
                if (!_currentBatch.IsEmpty())
                {
                    newData = _currentBatch.ToArray();
                }
                if ((_lastBatch != null) && (!_lastBatch.IsEmpty()))
                {
                    oldData = _lastBatch.ToArray();
                }

                if ((newData != null) || (oldData != null) || _isForceOutput)
                {
                    if (InstrumentationHelper.ENABLED)
                    {
                        InstrumentationHelper.Get().QViewIndicate(this, _timeBatchViewFactory.ViewName, newData, oldData);
                    }
                    UpdateChildren(newData, oldData);
                    if (InstrumentationHelper.ENABLED)
                    {
                        InstrumentationHelper.Get().AViewIndicate();
                    }
                }
            }

            // Only if forceOutput is enabled or
            // there have been any events in this or the last interval do we schedule a callback,
            // such as to not waste resources when no events arrive.
            if ((!_currentBatch.IsEmpty()) || ((_lastBatch != null) && (!_lastBatch.IsEmpty()))
                ||
                _isForceOutput)
            {
                ScheduleCallback();
                _isCallbackScheduled = true;
            }

            _lastBatch = _currentBatch;
            _currentBatch = new LinkedHashSet<EventBean>();
        }

        /// <summary>
        /// Returns true if the window is empty, or false if not empty.
        /// </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty()
        {
            if (_lastBatch != null)
            {
                if (!_lastBatch.IsEmpty())
                {
                    return false;
                }
            }
            return _currentBatch.IsEmpty();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _currentBatch.GetEnumerator();
        }

        public override String ToString()
        {
            return GetType().FullName +
                   " initialReferencePoint=" + _initialReferencePoint;
        }

        protected void ScheduleCallback()
        {
            long current = _agentInstanceContext.StatementContext.SchedulingService.Time;
            ExprTimePeriodEvalDeltaResult deltaWReference = _timeDeltaComputation.DeltaAddWReference(current, _currentReferencePoint.Value);
            long afterTime = deltaWReference.Delta;
            _currentReferencePoint = deltaWReference.LastReference;

            var callback = new ProxyScheduleHandleCallback()
            {
                ProcScheduledTrigger = (extensionServicesContext) =>
                {
                    if (InstrumentationHelper.ENABLED)
                    {
                        InstrumentationHelper.Get().QViewScheduledEval(this, _timeBatchViewFactory.ViewName);
                    }
                    SendBatch();
                    if (InstrumentationHelper.ENABLED)
                    {
                        InstrumentationHelper.Get().AViewScheduledEval();
                    }
                }
            };
            _handle = new EPStatementHandleCallback(_agentInstanceContext.EpStatementAgentInstanceHandle, callback);
            _agentInstanceContext.StatementContext.SchedulingService.Add(afterTime, _handle, _scheduleSlot);
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

        public void InternalHandleRemoved(EventBean eventBean)
        {
            // no action required
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(_currentBatch, true, _timeBatchViewFactory.ViewName, null);
            viewDataVisitor.VisitPrimary(_lastBatch, true, _timeBatchViewFactory.ViewName, null);
        }

        public ViewFactory ViewFactory => _timeBatchViewFactory;
    }
} // end of namespace
