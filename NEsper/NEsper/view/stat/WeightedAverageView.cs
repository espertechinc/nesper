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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.view.stat
{
    /// <summary>
    /// View for computing a weighted average. The view uses 2 fields within the parent view to compute the weighted average.
    /// The X field and weight field. In a price-volume example it calculates the volume-weighted average price
    /// as   (sum(price * volume) / sum(volume)).
    /// Example: weighted_avg("price", "volume")
    /// </summary>
    public class WeightedAverageView : ViewSupport, CloneableView, DerivedValueView
    {
        private readonly AgentInstanceViewFactoryChainContext _agentInstanceContext;
        private readonly ExprEvaluator _fieldNameXEvaluator;
        private readonly ExprEvaluator _fieldNameWeightEvaluator;

        private readonly EventBean[] _eventsPerStream = new EventBean[1];

        private EventBean _lastNewEvent;
        private WeightedAverageViewFactory _viewFactory;

        /// <summary>
        /// Constructor requires the name of the field to use in the parent view to compute the weighted average on,
        /// as well as the name of the field in the parent view to get the weight from.
        /// compute the average for.
        /// </summary>
        public WeightedAverageView(WeightedAverageViewFactory viewFactory, AgentInstanceViewFactoryChainContext agentInstanceContext)
        {
            CurrentValue = Double.NaN;
            SumW = Double.NaN;
            SumXtimesW = Double.NaN;
            _viewFactory = viewFactory;
            _fieldNameXEvaluator = viewFactory.FieldNameX.ExprEvaluator;
            _fieldNameWeightEvaluator = viewFactory.FieldNameWeight.ExprEvaluator;
            _agentInstanceContext = agentInstanceContext;
        }

        public View CloneView()
        {
            return ViewFactory.MakeView(_agentInstanceContext);
        }

        /// <summary>
        /// Returns the expression supplying the X values.
        /// </summary>
        /// <value>expression supplying X data points</value>
        public ExprNode FieldNameX
        {
            get { return _viewFactory.FieldNameX; }
        }

        /// <summary>
        /// Returns the expression supplying the weight values.
        /// </summary>
        /// <value>expression supplying weight</value>
        public ExprNode FieldNameWeight
        {
            get { return _viewFactory.FieldNameWeight; }
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewProcessIRStream(this, WeightedAverageViewFactory.NAME, newData, oldData); }

            double oldValue = CurrentValue;

            // If we have child views, keep a reference to the old values, so we can update them as old data event.
            EventBean oldDataMap = null;
            if (_lastNewEvent == null)
            {
                if (HasViews)
                {
                    IDictionary<string, object> oldDataValues = new Dictionary<string, object>();
                    oldDataValues.Put(ViewFieldEnum.WEIGHTED_AVERAGE__AVERAGE.GetName(), oldValue);
                    AddProperties(oldDataValues);
                    oldDataMap = _agentInstanceContext.StatementContext.EventAdapterService.AdapterForTypedMap(oldDataValues, ViewFactory.EventType);
                }
            }

            // add data points to the bean
            if (newData != null)
            {
                var evaluateParams = new EvaluateParams(_eventsPerStream, true, _agentInstanceContext);
                for (int i = 0; i < newData.Length; i++)
                {
                    _eventsPerStream[0] = newData[i];
                    var pointnum = _fieldNameXEvaluator.Evaluate(evaluateParams);
                    var weightnum = _fieldNameWeightEvaluator.Evaluate(evaluateParams);
                    if (pointnum != null && weightnum != null)
                    {
                        double point = pointnum.AsDouble();
                        double weight = weightnum.AsDouble();

                        if (double.IsNaN(SumXtimesW))
                        {
                            SumXtimesW = point * weight;
                            SumW = weight;
                        }
                        else
                        {
                            SumXtimesW += point * weight;
                            SumW += weight;
                        }
                    }
                }

                if ((_viewFactory.AdditionalProps != null) && (newData.Length != 0))
                {
                    if (LastValuesEventNew == null)
                    {
                        LastValuesEventNew = new object[_viewFactory.AdditionalProps.AdditionalExpr.Length];
                    }
                    for (int val = 0; val < _viewFactory.AdditionalProps.AdditionalExpr.Length; val++)
                    {
                        LastValuesEventNew[val] = _viewFactory.AdditionalProps.AdditionalExpr[val].Evaluate(
                            new EvaluateParams(_eventsPerStream, true, _agentInstanceContext));
                    }
                }
            }

            // remove data points from the bean
            if (oldData != null)
            {
                var evaluateParams = new EvaluateParams(_eventsPerStream, true, _agentInstanceContext);

                for (int i = 0; i < oldData.Length; i++)
                {
                    _eventsPerStream[0] = oldData[i];
                    var pointnum = _fieldNameXEvaluator.Evaluate(evaluateParams);
                    var weightnum = _fieldNameWeightEvaluator.Evaluate(evaluateParams);

                    if (pointnum != null && weightnum != null)
                    {
                        double point = pointnum.AsDouble();
                        double weight = weightnum.AsDouble();
                        SumXtimesW -= point * weight;
                        SumW -= weight;
                    }
                }
            }

            if (SumW != 0)
            {
                CurrentValue = SumXtimesW / SumW;
            }
            else
            {
                CurrentValue = Double.NaN;
            }

            // If there are child view, fireStatementStopped update method
            if (HasViews)
            {
                IDictionary<string, object> newDataMap = new Dictionary<string, object>();
                newDataMap.Put(ViewFieldEnum.WEIGHTED_AVERAGE__AVERAGE.GetName(), CurrentValue);
                AddProperties(newDataMap);
                EventBean newDataEvent = _agentInstanceContext.StatementContext.EventAdapterService.AdapterForTypedMap(newDataMap, ViewFactory.EventType);

                EventBean[] newEvents = new EventBean[] { newDataEvent };
                EventBean[] oldEvents;
                if (_lastNewEvent == null)
                {
                    oldEvents = new EventBean[] { oldDataMap };
                }
                else
                {
                    oldEvents = new EventBean[] { _lastNewEvent };
                }

                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewIndicate(this, WeightedAverageViewFactory.NAME, newEvents, oldEvents); }
                UpdateChildren(newEvents, oldEvents);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewIndicate(); }

                _lastNewEvent = newDataEvent;
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewProcessIRStream(); }
        }

        private void AddProperties(IDictionary<string, object> newDataMap)
        {
            if (_viewFactory.AdditionalProps == null)
            {
                return;
            }
            _viewFactory.AdditionalProps.AddProperties(newDataMap, LastValuesEventNew);
        }

        public override EventType EventType
        {
            get { return ViewFactory.EventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            var newDataMap = new Dictionary<string, object>();
            newDataMap.Put(ViewFieldEnum.WEIGHTED_AVERAGE__AVERAGE.GetName(), CurrentValue);
            AddProperties(newDataMap);
            var eventBean = _agentInstanceContext.StatementContext.EventAdapterService.AdapterForTypedMap(
                newDataMap, _viewFactory.EventType);
            if (eventBean != null)
                yield return eventBean;
        }

        public override string ToString()
        {
            return GetType().Name +
                    " fieldName=" + _viewFactory.FieldNameX +
                    " fieldNameWeight=" + _viewFactory.FieldNameWeight;
        }

        /// <summary>
        /// Creates the event type for this view.
        /// </summary>
        /// <param name="statementContext">is the event adapter service</param>
        /// <param name="additionalProps">The additional props.</param>
        /// <param name="streamNum">The stream number.</param>
        /// <returns>
        /// event type of view
        /// </returns>
        public static EventType CreateEventType(StatementContext statementContext, StatViewAdditionalProps additionalProps, int streamNum)
        {
            IDictionary<string, object> schemaMap = new Dictionary<string, object>();
            schemaMap.Put(ViewFieldEnum.WEIGHTED_AVERAGE__AVERAGE.GetName(), typeof(double?));
            StatViewAdditionalProps.AddCheckDupProperties(schemaMap, additionalProps, ViewFieldEnum.WEIGHTED_AVERAGE__AVERAGE);
            string outputEventTypeName = statementContext.StatementId + "_wavgview_" + streamNum;
            return statementContext.EventAdapterService.CreateAnonymousMapType(outputEventTypeName, schemaMap, false);
        }

        public double SumXtimesW { get; set; }

        public double SumW { get; set; }

        public double CurrentValue { get; set; }

        public object[] LastValuesEventNew { get; set; }

        public ViewFactory ViewFactory
        {
            get { return _viewFactory; }
        }
    }
} // end of namespace
