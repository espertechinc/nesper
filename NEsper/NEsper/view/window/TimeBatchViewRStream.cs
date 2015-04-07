///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// Same as the <seealso cref="TimeBatchView"/>, this view also supports fast-remove from the batch for remove stream events.
    /// </summary>
    public class TimeBatchViewRStream
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
        protected readonly ScheduleSlot ScheduleSlot;
        private readonly bool _isForceOutput;
        private readonly bool _isStartEager;
        protected EPStatementHandleCallback Handle;

        // Current running parameters
        protected long? CurrentReferencePoint;
        protected LinkedHashSet<EventBean> LastBatch = null;
        protected LinkedHashSet<EventBean> CurrentBatch = new LinkedHashSet<EventBean>();
        protected bool IsCallbackScheduled;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="timeBatchViewFactory">fr copying this view in a group-by</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <param name="timeDeltaComputation">is the number of milliseconds to batch events for</param>
        /// <param name="referencePoint">is the reference point onto which to base intervals, or null ifthere is no such reference point supplied</param>
        /// <param name="forceOutput">is true if the batch should produce empty output if there is no value to output following time intervals</param>
        /// <param name="isStartEager">is true for start-eager</param>
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

            ScheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();

            // schedule the first callback
            if (_isStartEager)
            {
                CurrentReferencePoint = agentInstanceContext.StatementContext.SchedulingService.Time;
                ScheduleCallback();
                IsCallbackScheduled = true;
            }
            agentInstanceContext.AddTerminationCallback(Stop);
        }

        public View CloneView()
        {
            return _timeBatchViewFactory.MakeView(_agentInstanceContext);
        }

        /// <summary>Returns the interval size in milliseconds. </summary>
        /// <value>batch size</value>
        public ExprTimePeriodEvalDeltaConst TimeDeltaComputation
        {
            get { return _timeDeltaComputation; }
        }

        /// <summary>Gets the reference point to use to anchor interval start and end dates to. </summary>
        /// <value>is the millisecond reference point.</value>
        public long? InitialReferencePoint
        {
            get { return _initialReferencePoint; }
        }

        /// <summary>True for force-output. </summary>
        /// <value>indicates force-output</value>
        public bool IsForceOutput
        {
            get { return _isForceOutput; }
        }

        /// <summary>True for start-eager. </summary>
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
            using (Instrument.With(
                i => i.QViewProcessIRStream(this, _timeBatchViewFactory.ViewName, newData, oldData),
                i => i.AViewProcessIRStream()))
            {
                if (oldData != null)
                {
                    for (int i = 0; i < oldData.Length; i++)
                    {
                        CurrentBatch.Remove(oldData[i]);
                        InternalHandleRemoved(oldData[i]);
                    }
                }

                // we don't care about removed data from a prior view
                if ((newData == null) || (newData.Length == 0))
                {
                    return;
                }

                // If we have an empty window about to be filled for the first time, schedule a callback
                if (CurrentBatch.IsEmpty())
                {
                    if (CurrentReferencePoint == null)
                    {
                        CurrentReferencePoint = _initialReferencePoint;
                        if (CurrentReferencePoint == null)
                        {
                            CurrentReferencePoint = _agentInstanceContext.StatementContext.SchedulingService.Time;
                        }
                    }

                    // Schedule the next callback if there is none currently scheduled
                    if (!IsCallbackScheduled)
                    {
                        ScheduleCallback();
                        IsCallbackScheduled = true;
                    }
                }

                // add data points to the timeWindow
                foreach (EventBean newEvent in newData)
                {
                    CurrentBatch.Add(newEvent);
                }

                // We do not Update child views, since we batch the events.
            }
        }

        /// <summary>
        /// This method updates child views and clears the batch of events. We schedule a 
        /// new callback at this time if there were events in the batch.
        /// </summary>
        protected void SendBatch()
        {
            IsCallbackScheduled = false;

            // If there are child views and the batch was filled, fireStatementStopped Update method
            if (HasViews)
            {
                // Convert to object arrays
                EventBean[] newData = null;
                EventBean[] oldData = null;
                if (CurrentBatch.IsNotEmpty())
                {
                    newData = CurrentBatch.ToArray();
                }
                if ((LastBatch != null) && (LastBatch.IsNotEmpty()))
                {
                    oldData = LastBatch.ToArray();
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
            if ((CurrentBatch.IsNotEmpty()) || ((LastBatch != null) && (LastBatch.IsNotEmpty()))
                ||
                (_isForceOutput))
            {
                ScheduleCallback();
                IsCallbackScheduled = true;
            }

            LastBatch = CurrentBatch;
            CurrentBatch = new LinkedHashSet<EventBean>();
        }

        /// <summary>Returns true if the window is empty, or false if not empty. </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty()
        {
            if (LastBatch != null)
            {
                if (LastBatch.IsNotEmpty())
                {
                    return false;
                }
            }
            return CurrentBatch.IsEmpty;
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return CurrentBatch.GetEnumerator();
        }

        public override String ToString()
        {
            return GetType().FullName +
                   " initialReferencePoint=" + _initialReferencePoint;
        }

        protected void ScheduleCallback()
        {
            var current = _agentInstanceContext.StatementContext.SchedulingService.Time;
            var deltaWReference = _timeDeltaComputation.DeltaMillisecondsAddWReference(
                current, CurrentReferencePoint.Value);
            var afterMSec = deltaWReference.Delta;
            CurrentReferencePoint = deltaWReference.LastReference;

            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback
            {
                ProcScheduledTrigger = extensionServicesContext => Instrument.With(
                    i => i.QViewScheduledEval(this, _timeBatchViewFactory.ViewName),
                    i => i.AViewScheduledEval(),
                    SendBatch)
            };
            Handle = new EPStatementHandleCallback(_agentInstanceContext.EpStatementAgentInstanceHandle, callback);
            _agentInstanceContext.StatementContext.SchedulingService.Add(afterMSec, Handle, ScheduleSlot);
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
            if (Handle != null)
            {
                _agentInstanceContext.StatementContext.SchedulingService.Remove(Handle, ScheduleSlot);
            }
        }

        public void InternalHandleRemoved(EventBean eventBean)
        {
            // no action required
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(CurrentBatch, true, _timeBatchViewFactory.ViewName, null);
            viewDataVisitor.VisitPrimary(LastBatch, true, _timeBatchViewFactory.ViewName, null);
        }

        public ViewFactory ViewFactory
        {
            get { return _timeBatchViewFactory; }
        }
    }
}
