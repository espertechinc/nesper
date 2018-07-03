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
    public class FirstTimeView
        : ViewSupport
        , CloneableView
        , StoppableView
        , DataWindowView
        , StopCallback
    {
        private readonly FirstTimeViewFactory _timeFirstViewFactory;
        protected readonly AgentInstanceViewFactoryChainContext AgentInstanceContext;
        private readonly ExprTimePeriodEvalDeltaConst _timeDeltaComputation;
        private readonly long _scheduleSlot;
        private EPStatementHandleCallback _handle;

        // Current running parameters
        private readonly LinkedHashSet<EventBean> _events = new LinkedHashSet<EventBean>();
        private bool _isClosed;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="timeFirstViewFactory">For copying this view in a group-by</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <param name="timeDeltaComputation">The time delta computation.</param>
        public FirstTimeView(
            FirstTimeViewFactory timeFirstViewFactory,
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            ExprTimePeriodEvalDeltaConst timeDeltaComputation)
        {
            AgentInstanceContext = agentInstanceContext;
            _timeFirstViewFactory = timeFirstViewFactory;
            _timeDeltaComputation = timeDeltaComputation;

            _scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();

            ScheduleCallback();

            agentInstanceContext.AddTerminationCallback(Stop);
        }

        public View CloneView()
        {
            return _timeFirstViewFactory.MakeView(AgentInstanceContext);
        }

        public ExprTimePeriodEvalDeltaConst TimeDeltaComputation => _timeDeltaComputation;

        public override EventType EventType => Parent.EventType;

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            using (Instrument.With(
                i => i.QViewProcessIRStream(this, _timeFirstViewFactory.ViewName, newData, oldData),
                i => i.AViewProcessIRStream()))
            {
                OneEventCollection oldDataToPost = null;
                if (oldData != null)
                {
                    foreach (EventBean anOldData in oldData)
                    {
                        bool removed = _events.Remove(anOldData);
                        if (removed)
                        {
                            if (oldDataToPost == null)
                            {
                                oldDataToPost = new OneEventCollection();
                            }
                            oldDataToPost.Add(anOldData);
                            InternalHandleRemoved(anOldData);
                        }
                    }
                }

                // add data points to the timeWindow
                OneEventCollection newDataToPost = null;
                if ((!_isClosed) && (newData != null))
                {
                    foreach (EventBean aNewData in newData)
                    {
                        _events.Add(aNewData);
                        if (newDataToPost == null)
                        {
                            newDataToPost = new OneEventCollection();
                        }
                        newDataToPost.Add(aNewData);
                        InternalHandleAdded(aNewData);
                    }
                }

                // If there are child views, call Update method
                if ((HasViews) && ((newDataToPost != null) || (oldDataToPost != null)))
                {
                    EventBean[] nd = (newDataToPost != null) ? newDataToPost.ToArray() : null;
                    EventBean[] od = (oldDataToPost != null) ? oldDataToPost.ToArray() : null;
                    Instrument.With(
                        i => i.QViewIndicate(this, _timeFirstViewFactory.ViewName, nd, od),
                        i => i.AViewIndicate(),
                        () => UpdateChildren(nd, od));
                }
            }
        }

        public void InternalHandleAdded(EventBean newEvent)
        {
            // no action
        }

        public void InternalHandleRemoved(EventBean oldEvent)
        {
            // no action
        }

        public void InternalHandleClosed()
        {
            // no action
        }

        /// <summary>Returns true if the window is empty, or false if not empty. </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty()
        {
            return _events.IsEmpty;
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _events.GetEnumerator();
        }

        public override String ToString()
        {
            return GetType().FullName;
        }

        private void ScheduleCallback()
        {
            long afterTime = _timeDeltaComputation.DeltaAdd(
                AgentInstanceContext.StatementContext.SchedulingService.Time);

            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback
            {
                ProcScheduledTrigger = extensionServicesContext => Instrument.With(
                    i => i.QViewScheduledEval(this, _timeFirstViewFactory.ViewName),
                    i => i.AViewScheduledEval(),
                    () =>
                    {
                        _isClosed = true;
                        InternalHandleClosed();
                    })
            };
            _handle = new EPStatementHandleCallback(AgentInstanceContext.EpStatementAgentInstanceHandle, callback);
            AgentInstanceContext.StatementContext.SchedulingService.Add(afterTime, _handle, _scheduleSlot);
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
            if (_handle != null)
            {
                AgentInstanceContext.StatementContext.SchedulingService.Remove(_handle, _scheduleSlot);
            }
        }

        public void SetClosed(bool closed)
        {
            _isClosed = closed;
        }

        public LinkedHashSet<EventBean> Events => _events;

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(_events, true, _timeFirstViewFactory.ViewName, null);
        }

        public ViewFactory ViewFactory => _timeFirstViewFactory;
    }
}
