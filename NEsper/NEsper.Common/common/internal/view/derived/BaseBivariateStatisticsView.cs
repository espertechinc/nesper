///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.view.derived
{
    /// <summary>
    ///     View for computing statistics that require 2 input variable arrays containing X and Y datapoints.
    ///     Subclasses compute correlation or regression values, for instance.
    /// </summary>
    public abstract class BaseBivariateStatisticsView : ViewSupport,
        DerivedValueView
    {
        private const string NAME = "Statistics";

        /// <summary>
        ///     Additional properties.
        /// </summary>
        protected internal readonly StatViewAdditionalPropsEval additionalProps;

        /// <summary>
        ///     Services required by implementing classes.
        /// </summary>
        protected internal readonly AgentInstanceContext agentInstanceContext;

        private readonly EventBean[] eventsPerStream = new EventBean[1];

        /// <summary>
        ///     Event type.
        /// </summary>
        protected internal readonly EventType eventType;

        protected internal readonly ViewFactory viewFactory;
        private EventBean lastNewEvent;

        private object[] _lastValuesEventNew;

        /// <summary>
        ///     This bean can be overridden by subclasses providing extra values such as correlation, regression.
        /// </summary>
        private BaseStatisticsBean _statisticsBean = new BaseStatisticsBean();

        /// <summary>
        ///     Constructor requires the name of the two fields to use in the parent view to compute the statistics.
        /// </summary>
        /// <param name="expressionXEval">is the expression to get the X values from</param>
        /// <param name="expressionYEval">is the expression to get the Y values from</param>
        /// <param name="agentInstanceContext">contains required view services</param>
        /// <param name="eventType">type of event</param>
        /// <param name="additionalProps">additional props</param>
        /// <param name="viewFactory">view factory</param>
        public BaseBivariateStatisticsView(
            ViewFactory viewFactory,
            AgentInstanceContext agentInstanceContext,
            ExprEvaluator expressionXEval,
            ExprEvaluator expressionYEval,
            EventType eventType,
            StatViewAdditionalPropsEval additionalProps
        )
        {
            this.viewFactory = viewFactory;
            this.agentInstanceContext = agentInstanceContext;
            ExpressionXEval = expressionXEval;
            ExpressionYEval = expressionYEval;
            this.eventType = eventType;
            this.additionalProps = additionalProps;
        }

        public BaseStatisticsBean StatisticsBean {
            get => _statisticsBean;
            internal set => _statisticsBean = value;
        }

        public object[] LastValuesEventNew {
            get => _lastValuesEventNew;
            set => _lastValuesEventNew = value;
        }

        public StatViewAdditionalPropsEval AdditionalProps => additionalProps;

        public ViewFactory ViewFactory => viewFactory;

        public ExprEvaluator ExpressionXEval { get; }

        public ExprEvaluator ExpressionYEval { get; }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, viewFactory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(viewFactory, newData, oldData);

            // If we have child views, keep a reference to the old values, so we can fireStatementStopped them as old data event.
            EventBean oldValues = null;
            if (lastNewEvent == null) {
                if (child != null) {
                    oldValues = PopulateMap(
                        _statisticsBean,
                        agentInstanceContext.EventBeanTypedEventFactory,
                        eventType,
                        additionalProps,
                        _lastValuesEventNew);
                }
            }

            // add data points to the bean
            if (newData != null) {
                for (var i = 0; i < newData.Length; i++) {
                    eventsPerStream[0] = newData[i];
                    var xnum = ExpressionXEval.Evaluate(eventsPerStream, true, agentInstanceContext);
                    var ynum = ExpressionYEval.Evaluate(eventsPerStream, true, agentInstanceContext);
                    if (xnum != null && ynum != null) {
                        double x = xnum.AsDouble();
                        double y = ynum.AsDouble();
                        _statisticsBean.AddPoint(x, y);
                    }
                }

                if (additionalProps != null && newData.Length != 0) {
                    var additionalEvals = additionalProps.AdditionalEvals;
                    if (_lastValuesEventNew == null) {
                        _lastValuesEventNew = new object[additionalEvals.Length];
                    }

                    for (var val = 0; val < additionalEvals.Length; val++) {
                        _lastValuesEventNew[val] = additionalEvals[val]
                            .Evaluate(
                                eventsPerStream,
                                true,
                                agentInstanceContext);
                    }
                }
            }

            // remove data points from the bean
            if (oldData != null) {
                for (var i = 0; i < oldData.Length; i++) {
                    eventsPerStream[0] = oldData[i];
                    var xnum = ExpressionXEval.Evaluate(eventsPerStream, true, agentInstanceContext);
                    var ynum = ExpressionYEval.Evaluate(eventsPerStream, true, agentInstanceContext);
                    if (xnum != null && ynum != null) {
                        double x = xnum.AsDouble();
                        double y = ynum.AsDouble();
                        _statisticsBean.RemovePoint(x, y);
                    }
                }
            }

            // If there are child view, fireStatementStopped update method
            if (child != null) {
                var newDataMap = PopulateMap(
                    _statisticsBean,
                    agentInstanceContext.EventBeanTypedEventFactory,
                    eventType,
                    additionalProps,
                    _lastValuesEventNew);
                EventBean[] newEvents = {newDataMap};
                EventBean[] oldEvents;
                if (lastNewEvent == null) {
                    oldEvents = new[] {oldValues};
                }
                else {
                    oldEvents = new[] {lastNewEvent};
                }

                agentInstanceContext.InstrumentationProvider.QViewIndicate(viewFactory, newEvents, oldEvents);
                child.Update(newEvents, oldEvents);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();

                lastNewEvent = newDataMap;
            }

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            EventBean value = PopulateMap(
                _statisticsBean,
                agentInstanceContext.EventBeanTypedEventFactory,
                eventType,
                additionalProps,
                _lastValuesEventNew);
            if (value != null) {
                yield return value;
            }
        }

        /// <summary>
        ///     Populate bean.
        /// </summary>
        /// <param name="baseStatisticsBean">results</param>
        /// <param name="eventAdapterService">event adapters</param>
        /// <param name="eventType">type</param>
        /// <param name="additionalProps">additional props</param>
        /// <param name="decoration">decoration values</param>
        /// <returns>bean</returns>
        protected internal abstract EventBean PopulateMap(
            BaseStatisticsBean baseStatisticsBean,
            EventBeanTypedEventFactory eventAdapterService,
            EventType eventType,
            StatViewAdditionalPropsEval additionalProps,
            object[] decoration);
    }
} // end of namespace