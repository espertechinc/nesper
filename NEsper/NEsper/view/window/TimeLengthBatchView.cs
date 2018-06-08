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
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// A data view that aggregates events in a stream and releases them in one batch if either 
    /// one of these conditions is reached, whichever comes first: One, a time interval passes. 
    /// Two, a given number of events collected. <para/>The view releases the batched events after 
    /// the interval or number of events as new data to child views. The prior batch if not empty 
    /// is released as old data to child view. The view DOES release intervals with no old or new 
    /// data. It does not collect old data published by a parent view. If there are no events in 
    /// the current and prior batch, the view WILL invoke the Update method of child views. 
    /// <para/>
    /// The view starts the first interval when the view is created.
    /// </summary>
    public class TimeLengthBatchView
        : ViewSupport
        , CloneableView
        , StoppableView
        , DataWindowView
        , StopCallback
    {
        // View parameters
        private readonly TimeLengthBatchViewFactory _timeLengthBatchViewFactory;
        private readonly AgentInstanceViewFactoryChainContext _agentInstanceContext;
        private readonly ExprTimePeriodEvalDeltaConst _timeDeltaComputation;
        private readonly long _numberOfEvents;
        private readonly bool _isForceOutput;
        private readonly bool _isStartEager;
        private readonly ViewUpdatedCollection _viewUpdatedCollection;
        private readonly long _scheduleSlot;

        // Current running parameters
        private List<EventBean> _lastBatch = null;
        private List<EventBean> _currentBatch = new List<EventBean>();
        private long? _callbackScheduledTime;
        private EPStatementHandleCallback _handle;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="timeBatchViewFactory">for copying this view in a group-by</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <param name="timeDeltaComputation">is the number of milliseconds to batch events for</param>
        /// <param name="numberOfEvents">is the event count before the batch fires off</param>
        /// <param name="forceOutput">is true if the batch should produce empty output if there is no value to output following time intervals</param>
        /// <param name="isStartEager">is true for start-eager</param>
        /// <param name="viewUpdatedCollection">is a collection that the view must Update when receiving events</param>
        public TimeLengthBatchView(
            TimeLengthBatchViewFactory timeBatchViewFactory,
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            ExprTimePeriodEvalDeltaConst timeDeltaComputation,
            long numberOfEvents,
            bool forceOutput,
            bool isStartEager,
            ViewUpdatedCollection viewUpdatedCollection)
        {
            _agentInstanceContext = agentInstanceContext;
            _timeLengthBatchViewFactory = timeBatchViewFactory;
            _timeDeltaComputation = timeDeltaComputation;
            _numberOfEvents = numberOfEvents;
            _isStartEager = isStartEager;
            _viewUpdatedCollection = viewUpdatedCollection;
            _isForceOutput = forceOutput;

            _scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();

            // schedule the first callback
            if (isStartEager)
            {
                ScheduleCallback(0);
            }

            agentInstanceContext.AddTerminationCallback(Stop);
        }

        public View CloneView()
        {
            return _timeLengthBatchViewFactory.MakeView(_agentInstanceContext);
        }

        /// <summary>Returns the interval size in milliseconds. </summary>
        /// <value>batch size</value>
        public ExprTimePeriodEvalDeltaConst TimeDeltaComputation => _timeDeltaComputation;

        /// <summary>True for force-output. </summary>
        /// <value>indicates force-output</value>
        public bool IsForceOutput => _isForceOutput;

        /// <summary>Returns the length of the batch. </summary>
        /// <value>maximum number of events allowed before window gets flushed</value>
        public long NumberOfEvents => _numberOfEvents;

        /// <summary>True for start-eager. </summary>
        /// <value>indicates start-eager</value>
        public bool IsStartEager => _isStartEager;

        public override EventType EventType => Parent.EventType;

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            using (Instrument.With(
                i => i.QViewProcessIRStream(this, _timeLengthBatchViewFactory.ViewName, newData, oldData),
                i => i.AViewProcessIRStream()))
            {
                if (oldData != null)
                {
                    for (int i = 0; i < oldData.Length; i++)
                    {
                        if (_currentBatch.Remove(oldData[i]))
                        {
                            InternalHandleRemoved(oldData[i]);
                        }
                    }
                }

                // we don't care about removed data from a prior view
                if ((newData == null) || (newData.Length == 0))
                {
                    return;
                }

                // Add data points
                foreach (EventBean newEvent in newData)
                {
                    _currentBatch.Add(newEvent);
                    InternalHandleAdded(newEvent);
                }

                // We are done unless we went over the boundary
                if (_currentBatch.Count < _numberOfEvents)
                {
                    // Schedule a callback if there is none scheduled
                    if (_callbackScheduledTime == null)
                    {
                        ScheduleCallback(0);
                    }

                    return;
                }

                // send a batch of events
                SendBatch(false);
            }
        }

        public void InternalHandleAdded(EventBean newEvent)
        {
            // no action required
        }

        public void InternalHandleRemoved(EventBean eventBean)
        {
            // no action required
        }

        /// <summary>
        /// This method updates child views and clears the batch of events. We cancel and 
        /// old callback and schedule a new callback at this time if there were events in 
        /// the batch.
        /// </summary>
        /// <param name="isFromSchedule">true if invoked from a schedule, false if not</param>
        protected void SendBatch(bool isFromSchedule)
        {
            // No more callbacks scheduled if called from a schedule
            if (isFromSchedule)
            {
                _callbackScheduledTime = null;
            }
            else
            {
                // Remove schedule if called from on overflow due to number of events
                if (_callbackScheduledTime != null)
                {
                    _agentInstanceContext.StatementContext.SchedulingService.Remove(_handle, _scheduleSlot);
                    _callbackScheduledTime = null;
                }
            }

            // If there are child views and the batch was filled, fireStatementStopped Update method
            if (HasViews)
            {
                // Convert to object arrays
                EventBean[] newData = null;
                EventBean[] oldData = null;
                if (_currentBatch.IsNotEmpty())
                {
                    newData = _currentBatch.ToArray();
                }
                if ((_lastBatch != null) && (_lastBatch.IsNotEmpty()))
                {
                    oldData = _lastBatch.ToArray();
                }

                // Post new data (current batch) and old data (prior batch)
                if (_viewUpdatedCollection != null)
                {
                    _viewUpdatedCollection.Update(newData, oldData);
                }
                if ((newData != null) || (oldData != null) || (_isForceOutput))
                {
                    using (Instrument.With(
                        i => i.QViewIndicate(this, _timeLengthBatchViewFactory.ViewName, newData, oldData),
                        i => i.AViewIndicate()))
                    {
                        UpdateChildren(newData, oldData);
                    }
                }
            }

            // Only if there have been any events in this or the last interval do we schedule a callback,
            // such as to not waste resources when no events arrive.
            if (((_currentBatch.IsNotEmpty()) || ((_lastBatch != null) && (_lastBatch.IsNotEmpty()))) || (_isForceOutput))
            {
                ScheduleCallback(0);
            }

            // Flush and roll
            _lastBatch = _currentBatch;
            _currentBatch = new List<EventBean>();
        }

        /// <summary>Returns true if the window is empty, or false if not empty. </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty()
        {
            if (_lastBatch != null)
            {
                if (_lastBatch.IsNotEmpty())
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
                    " numberOfEvents=" + _numberOfEvents;
        }

        protected void ScheduleCallback(long delta)
        {
            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback
            {
                ProcScheduledTrigger = extensionServicesContext => Instrument.With(
                    i => i.QViewScheduledEval(this, _timeLengthBatchViewFactory.ViewName),
                    i => i.AViewScheduledEval(),
                    () => SendBatch(true))
            };
            _handle = new EPStatementHandleCallback(_agentInstanceContext.EpStatementAgentInstanceHandle, callback);
            var currentTime = _agentInstanceContext.StatementContext.SchedulingService.Time;
            var scheduled = _timeDeltaComputation.DeltaAdd(currentTime) - delta;
            _agentInstanceContext.StatementContext.SchedulingService.Add(scheduled, _handle, _scheduleSlot);
            _callbackScheduledTime = _agentInstanceContext.StatementContext.SchedulingService.Time + scheduled;
        }

        public void StopView()
        {
            StopSchedule();
            _agentInstanceContext.RemoveTerminationCallback(Stop);
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
            viewDataVisitor.VisitPrimary(_lastBatch, true, _timeLengthBatchViewFactory.ViewName, null);
            viewDataVisitor.VisitPrimary(_currentBatch, true, _timeLengthBatchViewFactory.ViewName, null);
        }

        public ViewFactory ViewFactory => _timeLengthBatchViewFactory;
    }
}
