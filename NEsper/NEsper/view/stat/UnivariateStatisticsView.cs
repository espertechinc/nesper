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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.view.stat
{
    /// <summary>
    /// View for computing statistics, which the view exposes via fields representing the sum, 
    /// count, standard deviation for sample and for population and variance.
    /// </summary>
    public class UnivariateStatisticsView 
        : ViewSupport
        , CloneableView
        , DerivedValueView
    {
        private readonly UnivariateStatisticsViewFactory _viewFactory;
        private readonly AgentInstanceViewFactoryChainContext _agentInstanceContext;
        private readonly ExprEvaluator _fieldExpressionEvaluator;
        private readonly BaseStatisticsBean _baseStatisticsBean = new BaseStatisticsBean();
    
        private EventBean _lastNewEvent;
        private readonly EventBean[] _eventsPerStream = new EventBean[1];
        private Object[] _lastValuesEventNew;

        /// <summary>
        /// Constructor requires the name of the field to use in the parent view to compute the statistics.
        /// </summary>
        /// <param name="viewFactory">The view factory.</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        public UnivariateStatisticsView(UnivariateStatisticsViewFactory viewFactory, AgentInstanceViewFactoryChainContext agentInstanceContext)
        {
            this._viewFactory = viewFactory;
            this._agentInstanceContext = agentInstanceContext;
            this._fieldExpressionEvaluator = viewFactory.FieldExpression.ExprEvaluator;
        }
    
        public View CloneView()
        {
            return _viewFactory.MakeView(_agentInstanceContext);
        }

        /// <summary>Returns field name of the field to report statistics on. </summary>
        /// <value>field name</value>
        public ExprNode FieldExpression
        {
            get { return _viewFactory.FieldExpression; }
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewProcessIRStream(this, UnivariateStatisticsViewFactory.NAME, newData, oldData);}
    
            // If we have child views, keep a reference to the old values, so we can Update them as old data event.
            EventBean oldDataMap = null;
            if (_lastNewEvent == null)
            {
                if (HasViews)
                {
                    oldDataMap = PopulateMap(_baseStatisticsBean, _agentInstanceContext.StatementContext.EventAdapterService, _viewFactory.EventType, _viewFactory.AdditionalProps, _lastValuesEventNew);
                }
            }

            var evaluateParams = new EvaluateParams(_eventsPerStream, true, _agentInstanceContext);

            // add data points to the bean
            if (newData != null)
            {
                for (int i = 0; i < newData.Length; i++)
                {
                    _eventsPerStream[0] = newData[i];
                    var pointnum = _fieldExpressionEvaluator.Evaluate(evaluateParams);
                    if (pointnum != null) {
                        double point = pointnum.AsDouble();
                        _baseStatisticsBean.AddPoint(point, 0);
                    }
                }

                if ((_viewFactory.AdditionalProps != null) && (newData.Length != 0))
                {
                    if (_lastValuesEventNew == null)
                    {
                        _lastValuesEventNew = new Object[_viewFactory.AdditionalProps.AdditionalExpr.Length];
                    }
                    for (int val = 0; val < _viewFactory.AdditionalProps.AdditionalExpr.Length; val++)
                    {
                        _lastValuesEventNew[val] = _viewFactory.AdditionalProps.AdditionalExpr[val].Evaluate(evaluateParams);
                    }
                }
            }
    
            // remove data points from the bean
            if (oldData != null)
            {
                for (int i = 0; i < oldData.Length; i++)
                {
                    _eventsPerStream[0] = oldData[i];
                    var pointnum = _fieldExpressionEvaluator.Evaluate(evaluateParams);
                    if (pointnum != null) {
                        double point = pointnum.AsDouble();
                        _baseStatisticsBean.RemovePoint(point, 0);
                    }
                }
            }
    
            // If there are child view, call Update method
            if (HasViews)
            {
                EventBean newDataMap = PopulateMap(_baseStatisticsBean, _agentInstanceContext.StatementContext.EventAdapterService, _viewFactory.EventType, _viewFactory.AdditionalProps, _lastValuesEventNew);
    
                EventBean[] oldEvents;
                EventBean[] newEvents = new EventBean[] {newDataMap};
                if (_lastNewEvent == null) {
                    oldEvents = new EventBean[] {oldDataMap};
                }
                else {
                    oldEvents = new EventBean[] {_lastNewEvent};
                }

                Instrument.With(
                    i => i.QViewIndicate(this, UnivariateStatisticsViewFactory.NAME, newEvents, oldEvents),
                    i => i.AViewIndicate(),
                    () => UpdateChildren(newEvents, oldEvents));
    
                _lastNewEvent = newDataMap;
            }
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewProcessIRStream();}
        }

        public override EventType EventType
        {
            get { return _viewFactory.EventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            yield return PopulateMap(
                _baseStatisticsBean,
                _agentInstanceContext.StatementContext.EventAdapterService,
                _viewFactory.EventType,
                _viewFactory.AdditionalProps, _lastValuesEventNew);
        }
    
        public override String ToString()
        {
            return GetType().FullName + " fieldExpression=" + _viewFactory.FieldExpression;
        }
    
        public static EventBean PopulateMap(BaseStatisticsBean baseStatisticsBean,
                                      EventAdapterService eventAdapterService,
                                      EventType eventType,
                                      StatViewAdditionalProps additionalProps,
                                      Object[] lastNewValues)
        {
            var result = new Dictionary<string, object>();
            result.Put(ViewFieldEnum.UNIVARIATE_STATISTICS__DATAPOINTS.GetName(), baseStatisticsBean.N);
            result.Put(ViewFieldEnum.UNIVARIATE_STATISTICS__TOTAL.GetName(), baseStatisticsBean.XSum);
            result.Put(ViewFieldEnum.UNIVARIATE_STATISTICS__STDDEV.GetName(), baseStatisticsBean.XStandardDeviationSample);
            result.Put(ViewFieldEnum.UNIVARIATE_STATISTICS__STDDEVPA.GetName(), baseStatisticsBean.XStandardDeviationPop);
            result.Put(ViewFieldEnum.UNIVARIATE_STATISTICS__VARIANCE.GetName(), baseStatisticsBean.XVariance);
            result.Put(ViewFieldEnum.UNIVARIATE_STATISTICS__AVERAGE.GetName(), baseStatisticsBean.XAverage);
            if (additionalProps != null) {
                additionalProps.AddProperties(result, lastNewValues);
            }
            return eventAdapterService.AdapterForTypedMap(result, eventType);
        }

        /// <summary>
        /// Creates the event type for this view.
        /// </summary>
        /// <param name="statementContext">is the event adapter service</param>
        /// <param name="additionalProps">The additional props.</param>
        /// <param name="streamNum">The stream num.</param>
        /// <returns>event type of view</returns>
        public static EventType CreateEventType(StatementContext statementContext, StatViewAdditionalProps additionalProps, int streamNum)
        {
            IDictionary<String, Object> eventTypeMap = new Dictionary<string, object>();
            eventTypeMap.Put(ViewFieldEnum.UNIVARIATE_STATISTICS__DATAPOINTS.GetName(), typeof(long?));
            eventTypeMap.Put(ViewFieldEnum.UNIVARIATE_STATISTICS__TOTAL.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.UNIVARIATE_STATISTICS__STDDEV.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.UNIVARIATE_STATISTICS__STDDEVPA.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.UNIVARIATE_STATISTICS__VARIANCE.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.UNIVARIATE_STATISTICS__AVERAGE.GetName(), typeof(double?));
            StatViewAdditionalProps.AddCheckDupProperties(eventTypeMap, additionalProps,
                    ViewFieldEnum.UNIVARIATE_STATISTICS__DATAPOINTS,
                    ViewFieldEnum.UNIVARIATE_STATISTICS__TOTAL,
                    ViewFieldEnum.UNIVARIATE_STATISTICS__STDDEV,
                    ViewFieldEnum.UNIVARIATE_STATISTICS__STDDEVPA,
                    ViewFieldEnum.UNIVARIATE_STATISTICS__VARIANCE,
                    ViewFieldEnum.UNIVARIATE_STATISTICS__AVERAGE
                    );
            String outputEventTypeName = statementContext.StatementId + "_statview_" + streamNum;
            return statementContext.EventAdapterService.CreateAnonymousMapType(outputEventTypeName, eventTypeMap, false);
        }

        public BaseStatisticsBean BaseStatisticsBean
        {
            get { return _baseStatisticsBean; }
        }

        public object[] LastValuesEventNew
        {
            get { return _lastValuesEventNew; }
            set { _lastValuesEventNew = value; }
        }

        public ViewFactory ViewFactory
        {
            get { return _viewFactory; }
        }
    }
}
