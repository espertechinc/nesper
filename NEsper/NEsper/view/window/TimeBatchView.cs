///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// A data view that aggregates events in a stream and releases them in one batch at 
    /// every specified time interval. The view works similar to a time_window but in not 
    /// continuous. The view releases the batched events after the interval as new data to 
    /// child views. The prior batch if not empty is released as old data to child view. 
    /// The view doesn't release intervals with no old or new data. It also does not collect 
    /// old data published by a parent view. For example, we want to calculate the average 
    /// of IBM stock every hour, for the last hour. The view accepts 2 parameter combinations.
    ///  (1) A time interval is supplied with a reference point - based on this point the intervals are set.
    ///  (1) A time interval is supplied but no reference point - the reference point is set when 
    ///      the first event arrives.  If there are no events in the current and prior batch, the 
    ///      view will not invoke the Update method of child views. 
    /// In that case also, no next callback is scheduled with the scheduling service until the next event arrives.
    /// </summary>
    public class TimeBatchView 
        : ViewSupport
        , CloneableView
        , StoppableView
        , DataWindowView
    {
        // View parameters
        private readonly TimeBatchViewFactory _timeBatchViewFactory;
        private readonly AgentInstanceViewFactoryChainContext _agentInstanceContext;
        private readonly ExprTimePeriodEvalDeltaConst _timeDeltaComputation;
        private readonly long? _initialReferencePoint;
        private readonly bool _isForceOutput;
        private readonly bool _isStartEager;
        private readonly ViewUpdatedCollection _viewUpdatedCollection;
        private readonly ScheduleSlot _scheduleSlot;
        private EPStatementHandleCallback _handle;
    
        // Current running parameters
        private long? _currentReferencePoint;
        private ArrayDeque<EventBean> _lastBatch = null;
        private ArrayDeque<EventBean> _currentBatch = new ArrayDeque<EventBean>();
        private bool _isCallbackScheduled;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="timeBatchViewFactory">for copying this view in a group-by</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <param name="timeDeltaComputation">computes the number of milliseconds to batch events for</param>
        /// <param name="referencePoint">is the reference point onto which to base intervals, or null ifthere is no such reference point supplied</param>
        /// <param name="forceOutput">is true if the batch should produce empty output if there is no value to output following time intervals</param>
        /// <param name="isStartEager">is true for start-eager</param>
        /// <param name="viewUpdatedCollection">is a collection that the view must Update when receiving events</param>
        public TimeBatchView(TimeBatchViewFactory timeBatchViewFactory,
                             AgentInstanceViewFactoryChainContext agentInstanceContext,
                             ExprTimePeriodEvalDeltaConst timeDeltaComputation,
                             long? referencePoint,
                             bool forceOutput,
                             bool isStartEager,
                             ViewUpdatedCollection viewUpdatedCollection)
        {
            _agentInstanceContext = agentInstanceContext;
            _timeBatchViewFactory = timeBatchViewFactory;
            _timeDeltaComputation = timeDeltaComputation;
            _initialReferencePoint = referencePoint;
            _isStartEager = isStartEager;
            _viewUpdatedCollection = viewUpdatedCollection;
            _isForceOutput = forceOutput;
    
            _scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();
    
            // schedule the first callback
            if (isStartEager)
            {
                if (_currentReferencePoint == null)
                {
                    _currentReferencePoint = agentInstanceContext.StatementContext.SchedulingService.Time;
                }
                ScheduleCallback();
                _isCallbackScheduled = true;
            }
    
            agentInstanceContext.AddTerminationCallback(Stop);
        }
    
        public View CloneView()
        {
            return _timeBatchViewFactory.MakeView(_agentInstanceContext);
        }

        /// <summary>
        /// Returns the interval size in milliseconds.
        /// </summary>
        /// <value>batch size</value>
        public ExprTimePeriodEvalDeltaConst TimeDeltaComputation
        {
            get { return _timeDeltaComputation; }
        }

        /// <summary>
        /// Gets the reference point to use to anchor interval start and end dates to.
        /// </summary>
        /// <value>is the millisecond reference point.</value>
        public long? InitialReferencePoint
        {
            get { return _initialReferencePoint; }
        }

        /// <summary>
        /// True for force-output.
        /// </summary>
        /// <value>indicates force-output</value>
        public bool IsForceOutput
        {
            get { return _isForceOutput; }
        }

        /// <summary>
        /// True for start-eager.
        /// </summary>
        /// <value>indicates start-eager</value>
        public bool IsStartEager
        {
            get { return _isStartEager; }
        }

        public override EventType EventType
        {
            get { return Parent.EventType; }
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            using(Instrument.With(
                i => i.QViewProcessIRStream(this, _timeBatchViewFactory.ViewName, newData, oldData),
                i => i.AViewProcessIRStream()))
            {
                // we don't care about removed data from a prior view
                if ((newData == null) || (newData.Length == 0))
                {
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

                // We do not Update child views, since we batch the events.
            }
        }

        /// <summary>
        /// This method updates child views and clears the batch of events. We schedule a 
        /// new callback at this time if there were events in the batch.
        /// </summary>
        public void SendBatch()
        {
            _isCallbackScheduled = false;
    
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
                    Instrument.With(
                        i => i.QViewIndicate(this, _timeBatchViewFactory.ViewName, newData, oldData),
                        i => i.AViewIndicate(),
                        () => UpdateChildren(newData, oldData));
                }
            }
    
            // Only if forceOutput is enabled or
            // there have been any events in this or the last interval do we schedule a callback,
            // such as to not waste resources when no events arrive.
            if ((_currentBatch.IsNotEmpty()) || ((_lastBatch != null) && (_lastBatch.IsNotEmpty()))
                ||
                (_isForceOutput))
            {
                ScheduleCallback();
                _isCallbackScheduled = true;
            }
    
            _lastBatch = _currentBatch;
            _currentBatch = new ArrayDeque<EventBean>();
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
                    " initialReferencePoint=" + _initialReferencePoint;
        }
    
        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(_currentBatch, true, _timeBatchViewFactory.ViewName, null);
            viewDataVisitor.VisitPrimary(_lastBatch, true, _timeBatchViewFactory.ViewName, null);
        }
    
        protected void ScheduleCallback()
        {
            long current = _agentInstanceContext.StatementContext.SchedulingService.Time;
            ExprTimePeriodEvalDeltaResult deltaWReference = _timeDeltaComputation.DeltaMillisecondsAddWReference(
                current, _currentReferencePoint.Value);
            long afterMSec = deltaWReference.Delta;
            _currentReferencePoint = deltaWReference.LastReference;
    
            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback
            {
                ProcScheduledTrigger = extensionServicesContext => Instrument.With(
                    i => i.QViewScheduledEval(this, _timeBatchViewFactory.ViewName),
                    i => i.AViewScheduledEval(),
                    SendBatch)
            };
            _handle = new EPStatementHandleCallback(_agentInstanceContext.EpStatementAgentInstanceHandle, callback);
            _agentInstanceContext.StatementContext.SchedulingService.Add(afterMSec, _handle, _scheduleSlot);
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

        public ViewFactory ViewFactory
        {
            get { return _timeBatchViewFactory; }
        }
    }
}
