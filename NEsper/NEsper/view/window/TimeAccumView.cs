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
    /// A data window view that holds events in a stream and only removes events from a stream (rstream) 
    /// if no more events arrive for a given time interval. <para/>No batch version of the view exists 
    /// as the batch version is simply the remove stream of this view, which removes in batches.
    /// <para/>
    /// The view is continuous, the insert stream consists of arriving events. The remove stream only 
    /// posts current window contents when no more events arrive for a given timer interval.
    /// </summary>
    public class TimeAccumView
        : ViewSupport
        , CloneableView
        , DataWindowView
        , StoppableView
        , StopCallback
    {
        // View parameters
        private readonly TimeAccumViewFactory _factory;
        protected readonly AgentInstanceViewFactoryChainContext AgentInstanceContext;
        private readonly ExprTimePeriodEvalDeltaConst _timeDeltaComputation;
        protected readonly ViewUpdatedCollection ViewUpdatedCollection;
        protected readonly long ScheduleSlot;

        // Current running parameters
        protected List<EventBean> CurrentBatch = new List<EventBean>();
        protected long CallbackScheduledTime;
        protected EPStatementHandleCallback Handle;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="timeBatchViewFactory">fr copying this view in a group-by</param>
        /// <param name="agentInstanceContext">is required view services</param>
        /// <param name="timeDeltaComputation">The time delta computation.</param>
        /// <param name="viewUpdatedCollection">is a collection that the view must Update when receiving events</param>
        public TimeAccumView(
            TimeAccumViewFactory timeBatchViewFactory,
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            ExprTimePeriodEvalDeltaConst timeDeltaComputation,
            ViewUpdatedCollection viewUpdatedCollection)
        {
            AgentInstanceContext = agentInstanceContext;
            _factory = timeBatchViewFactory;
            _timeDeltaComputation = timeDeltaComputation;
            ViewUpdatedCollection = viewUpdatedCollection;

            ScheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();

            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback
            {
                ProcScheduledTrigger = extensionServicesContext => Instrument.With(
                    i => i.QViewScheduledEval(this, _factory.ViewName),
                    i => i.AViewScheduledEval(),
                    SendRemoveStream)
            };
            Handle = new EPStatementHandleCallback(agentInstanceContext.EpStatementAgentInstanceHandle, callback);
            agentInstanceContext.AddTerminationCallback(Stop);
        }

        public View CloneView()
        {
            return _factory.MakeView(AgentInstanceContext);
        }

        public ExprTimePeriodEvalDeltaConst TimeDeltaComputation
        {
            get { return _timeDeltaComputation; }
        }

        public override EventType EventType
        {
            get { return Parent.EventType; }
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            using (Instrument.With(
                i => i.QViewProcessIRStream(this, _factory.ViewName, newData, oldData),
                i => i.AViewProcessIRStream()))
            {
                // we don't care about removed data from a prior view
                if ((newData == null) || (newData.Length == 0))
                {
                    return;
                }

                // If we have an empty window about to be filled for the first time, addSchedule a callback
                bool removeSchedule = false;
                bool addSchedule = false;
                long timestamp = AgentInstanceContext.StatementContext.SchedulingService.Time;

                if (CurrentBatch.IsNotEmpty())
                {
                    // check if we need to reschedule
                    long callbackTime = timestamp + _timeDeltaComputation.DeltaMillisecondsAdd(timestamp); ;
                    if (callbackTime != CallbackScheduledTime)
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
                    AgentInstanceContext.StatementContext.SchedulingService.Remove(Handle, ScheduleSlot);
                }
                if (addSchedule)
                {
                    var msecIntervalSize = _timeDeltaComputation.DeltaMillisecondsAdd(timestamp);
                    AgentInstanceContext.StatementContext.SchedulingService.Add(msecIntervalSize, Handle, ScheduleSlot);
                    CallbackScheduledTime = msecIntervalSize + timestamp;
                }

                // add data points to the window
                foreach (EventBean newEvent in newData)
                {
                    CurrentBatch.Add(newEvent);
                }

                // forward insert stream to child views
                if (ViewUpdatedCollection != null)
                {
                    ViewUpdatedCollection.Update(newData, null);
                }

                // Update child views
                if (HasViews)
                {
                    Instrument.With(
                        i => i.QViewIndicate(this, _factory.ViewName, newData, null),
                        i => i.AViewIndicate(),
                        () => UpdateChildren(newData, null));
                }
            }
        }

        /// <summary>This method sends the remove stream for all accumulated events. </summary>
        protected void SendRemoveStream()
        {
            CallbackScheduledTime = -1;

            // If there are child views and the batch was filled, fireStatementStopped Update method
            if (HasViews)
            {
                // Convert to object arrays
                EventBean[] oldData = null;
                if (CurrentBatch.IsNotEmpty())
                {
                    oldData = CurrentBatch.ToArray();
                }

                // Post old data
                if (ViewUpdatedCollection != null)
                {
                    ViewUpdatedCollection.Update(null, oldData);
                }

                if (oldData != null)
                {
                    Instrument.With(
                        i => i.QViewIndicate(this, _factory.ViewName, null, oldData),
                        i => i.AViewIndicate(),
                        () => UpdateChildren(null, oldData));
                }
            }

            CurrentBatch.Clear();
        }

        /// <summary>Returns true if the window is empty, or false if not empty. </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty()
        {
            return CurrentBatch.IsEmpty();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return CurrentBatch.GetEnumerator();
        }

        public override String ToString()
        {
            return GetType().FullName;
        }

        public void StopView()
        {
            StopSchedule();
            AgentInstanceContext.RemoveTerminationCallback(Stop);
        }

        public void Stop()
        {
            StopSchedule();
        }

        public void StopSchedule()
        {
            if (Handle != null)
            {
                AgentInstanceContext.StatementContext.SchedulingService.Remove(Handle, ScheduleSlot);
            }
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(CurrentBatch, true, _factory.ViewName, null);
        }

        public ViewFactory ViewFactory
        {
            get { return _factory; }
        }
    }
}