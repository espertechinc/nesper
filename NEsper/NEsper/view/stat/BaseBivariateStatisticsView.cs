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
using com.espertech.esper.compat;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.view.stat
{
    /// <summary>
    /// View for computing statistics that require 2 input variable arrays containing X and Y datapoints.
    /// Subclasses compute correlation or regression values, for instance.
    /// </summary>
    public abstract class BaseBivariateStatisticsView : ViewSupport, DerivedValueView
    {
        private const String NAME = "Statistics";

        private readonly ViewFactory _viewFactory;

        /// <summary>This bean can be overridden by subclasses providing extra values such as correlation, regression. </summary>
        private BaseStatisticsBean _statisticsBean = new BaseStatisticsBean();
    
        private readonly ExprNode _expressionX;
        private readonly ExprNode _expressionY;
        private readonly ExprEvaluator _expressionXEval;
        private readonly ExprEvaluator _expressionYEval;
        private readonly EventBean[] _eventsPerStream = new EventBean[1];
    
        /// <summary>Services required by implementing classes. </summary>
        protected readonly AgentInstanceContext AgentInstanceContext;
    
        /// <summary>Additional properties. </summary>
        private readonly StatViewAdditionalProps _additionalProps;
    
        /// <summary>Event type. </summary>
        protected readonly EventType _eventType;

        private Object[] _lastValuesEventNew;
        private EventBean _lastNewEvent;
    
        /// <summary>Populate bean. </summary>
        /// <param name="baseStatisticsBean">results</param>
        /// <param name="eventAdapterService">event adapters</param>
        /// <param name="eventType">type</param>
        /// <param name="additionalProps">additional props</param>
        /// <param name="decoration">decoration values</param>
        /// <returns>bean</returns>
        protected abstract EventBean PopulateMap(BaseStatisticsBean baseStatisticsBean, EventAdapterService eventAdapterService, EventType eventType, StatViewAdditionalProps additionalProps, Object[] decoration);

        /// <summary>
        /// Constructor requires the name of the two fields to use in the parent view to compute the statistics.
        /// </summary>
        /// <param name="viewFactory">The view factory.</param>
        /// <param name="agentInstanceContext">contains required view services</param>
        /// <param name="expressionX">is the expression to get the X values from</param>
        /// <param name="expressionY">is the expression to get the Y values from</param>
        /// <param name="eventType">type of event</param>
        /// <param name="additionalProps">additional props</param>
        protected BaseBivariateStatisticsView(
            ViewFactory viewFactory,
            AgentInstanceContext agentInstanceContext,
            ExprNode expressionX,
            ExprNode expressionY,
            EventType eventType,
            StatViewAdditionalProps additionalProps)
        {
            _viewFactory = viewFactory;
            AgentInstanceContext = agentInstanceContext;
            _expressionX = expressionX;
            _expressionXEval = expressionX.ExprEvaluator;
            _expressionY = expressionY;
            _expressionYEval = expressionY.ExprEvaluator;
            _eventType = eventType;
            _additionalProps = additionalProps;
        }
    
        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            using(Instrument.With(
                i => i.QViewProcessIRStream(this, NAME, newData, oldData),
                i => i.AViewProcessIRStream()))
            {
                // If we have child views, keep a reference to the old values, so we can fireStatementStopped them as old data event.
                EventBean oldValues = null;
                if (_lastNewEvent == null)
                {
                    if (HasViews)
                    {
                        oldValues = PopulateMap(_statisticsBean, AgentInstanceContext.StatementContext.EventAdapterService, _eventType, _additionalProps, _lastValuesEventNew);
                    }
                }

                var evaluateParams = new EvaluateParams(_eventsPerStream, true, AgentInstanceContext);

                // add data points to the bean
                if (newData != null)
                {
                    for (var i = 0; i < newData.Length; i++)
                    {
                        _eventsPerStream[0] = newData[i];
                        var xnum = _expressionXEval.Evaluate(evaluateParams);
                        var ynum = _expressionYEval.Evaluate(evaluateParams);
                        if (xnum != null && ynum != null) {
                            var x = xnum.AsDouble();
                            var y = ynum.AsDouble();
                            _statisticsBean.AddPoint(x, y);
                        }
                    }
    
                    if ((_additionalProps != null) && (newData.Length != 0)) {
                        if (_lastValuesEventNew == null) {
                            _lastValuesEventNew = new Object[_additionalProps.AdditionalExpr.Length];
                        }
                        for (var val = 0; val < _additionalProps.AdditionalExpr.Length; val++)
                        {
                            _lastValuesEventNew[val] = _additionalProps.AdditionalExpr[val].Evaluate(evaluateParams);
                        }
                    }
                }
    
                // remove data points from the bean
                if (oldData != null)
                {
                    for (var i = 0; i < oldData.Length; i++)
                    {
                        _eventsPerStream[0] = oldData[i];
                        var xnum = _expressionXEval.Evaluate(evaluateParams);
                        var ynum = _expressionYEval.Evaluate(evaluateParams);
                        if (xnum != null && ynum != null) {
                            var x = xnum.AsDouble();
                            var y = ynum.AsDouble();
                            _statisticsBean.RemovePoint(x, y);
                        }
                    }
                }
    
                // If there are child view, fireStatementStopped Update method
                if (HasViews)
                {
                    var newDataMap = PopulateMap(_statisticsBean, AgentInstanceContext.StatementContext.EventAdapterService, _eventType, _additionalProps, _lastValuesEventNew);
                    var newEvents = new EventBean[] {newDataMap};
                    EventBean[] oldEvents;
                    if (_lastNewEvent == null) {
                        oldEvents = new EventBean[] {oldValues};
                    }
                    else {
                        oldEvents = new EventBean[] {_lastNewEvent};
                    }

                    Instrument.With(
                        i => i.QViewIndicate(this, NAME, newEvents, oldEvents),
                        i => i.AViewIndicate(),
                        () => UpdateChildren(newEvents, oldEvents));
    
                    _lastNewEvent = newDataMap;
                }
            }
        }
    
        public override IEnumerator<EventBean> GetEnumerator()
        {
            yield return PopulateMap(_statisticsBean,
                    AgentInstanceContext.StatementContext.EventAdapterService,
                    _eventType, _additionalProps, _lastValuesEventNew);
        }

        /// <summary>Returns the expression supplying X data points. </summary>
        /// <value>X expression</value>
        public ExprNode ExpressionX
        {
            get { return _expressionX; }
        }

        /// <summary>Returns the expression supplying Y data points. </summary>
        /// <value>Y expression</value>
        public ExprNode ExpressionY
        {
            get { return _expressionY; }
        }

        public BaseStatisticsBean StatisticsBean
        {
            get { return _statisticsBean; }
            internal set { _statisticsBean = value; }
        }

        public object[] LastValuesEventNew
        {
            get { return _lastValuesEventNew; }
            set { _lastValuesEventNew = value; }
        }

        public StatViewAdditionalProps AdditionalProps
        {
            get { return _additionalProps; }
        }

        public ViewFactory ViewFactory
        {
            get { return _viewFactory; }
        }
    }
}
