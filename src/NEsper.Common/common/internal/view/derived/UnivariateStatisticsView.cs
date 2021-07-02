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
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.derived
{
    /// <summary>
    /// View for computing statistics, which the view exposes via fields representing the sum, count, standard deviation
    /// for sample and for population and variance.
    /// </summary>
    public class UnivariateStatisticsView : ViewSupport,
        DerivedValueView
    {
        private readonly UnivariateStatisticsViewFactory _viewFactory;
        internal readonly AgentInstanceContext agentInstanceContext;
        internal readonly BaseStatisticsBean baseStatisticsBean = new BaseStatisticsBean();

        private EventBean _lastNewEvent;
        private readonly EventBean[] _eventsPerStream = new EventBean[1];
        protected object[] lastValuesEventNew;

        public UnivariateStatisticsView(
            UnivariateStatisticsViewFactory viewFactory,
            AgentInstanceViewFactoryChainContext agentInstanceContext)
        {
            this._viewFactory = viewFactory;
            this.agentInstanceContext = agentInstanceContext.AgentInstanceContext;
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, _viewFactory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(_viewFactory, newData, oldData);

            // If we have child views, keep a reference to the old values, so we can update them as old data event.
            EventBean oldDataMap = null;
            if (_lastNewEvent == null) {
                if (child != null) {
                    oldDataMap = PopulateMap(
                        baseStatisticsBean,
                        agentInstanceContext.EventBeanTypedEventFactory,
                        _viewFactory.eventType,
                        _viewFactory.additionalProps,
                        lastValuesEventNew);
                }
            }

            // add data points to the bean
            if (newData != null) {
                for (int i = 0; i < newData.Length; i++) {
                    _eventsPerStream[0] = newData[i];
                    var pointnum = _viewFactory.fieldEval.Evaluate(_eventsPerStream, true, agentInstanceContext);
                    if (pointnum != null) {
                        double point = pointnum.AsDouble();
                        baseStatisticsBean.AddPoint(point, 0);
                    }
                }

                if ((_viewFactory.additionalProps != null) && (newData.Length != 0)) {
                    var additionalEvals = _viewFactory.additionalProps.AdditionalEvals;
                    if (lastValuesEventNew == null) {
                        lastValuesEventNew = new object[additionalEvals.Length];
                    }

                    for (int val = 0; val < additionalEvals.Length; val++) {
                        lastValuesEventNew[val] =
                            additionalEvals[val].Evaluate(_eventsPerStream, true, agentInstanceContext);
                    }
                }
            }

            // remove data points from the bean
            if (oldData != null) {
                for (int i = 0; i < oldData.Length; i++) {
                    _eventsPerStream[0] = oldData[i];
                    var pointnum = _viewFactory.fieldEval.Evaluate(_eventsPerStream, true, agentInstanceContext);
                    if (pointnum != null) {
                        double point = pointnum.AsDouble();
                        baseStatisticsBean.RemovePoint(point, 0);
                    }
                }
            }

            // If there are child view, call update method
            if (child != null) {
                EventBean newDataMap = PopulateMap(
                    baseStatisticsBean,
                    agentInstanceContext.EventBeanTypedEventFactory,
                    _viewFactory.eventType,
                    _viewFactory.additionalProps,
                    lastValuesEventNew);

                EventBean[] oldEvents;
                EventBean[] newEvents = new EventBean[] {newDataMap};
                if (_lastNewEvent == null) {
                    oldEvents = new EventBean[] {oldDataMap};
                }
                else {
                    oldEvents = new EventBean[] {_lastNewEvent};
                }

                agentInstanceContext.InstrumentationProvider.QViewIndicate(_viewFactory, newEvents, oldEvents);
                child.Update(newEvents, oldEvents);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();

                _lastNewEvent = newDataMap;
            }

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override EventType EventType {
            get => _viewFactory.eventType;
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return EnumerationHelper.SingletonNullable(
                PopulateMap(
                    baseStatisticsBean,
                    agentInstanceContext.EventBeanTypedEventFactory,
                    _viewFactory.eventType,
                    _viewFactory.additionalProps,
                    lastValuesEventNew));
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        public static EventBean PopulateMap(
            BaseStatisticsBean baseStatisticsBean,
            EventBeanTypedEventFactory eventAdapterService,
            EventType eventType,
            StatViewAdditionalPropsEval additionalProps,
            object[] lastNewValues)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            result.Put(ViewFieldEnum.UNIVARIATE_STATISTICS_DATAPOINTS.GetName(), baseStatisticsBean.N);
            result.Put(ViewFieldEnum.UNIVARIATE_STATISTICS_TOTAL.GetName(), baseStatisticsBean.XSum);
            result.Put(
                ViewFieldEnum.UNIVARIATE_STATISTICS_STDDEV.GetName(),
                baseStatisticsBean.XStandardDeviationSample);
            result.Put(
                ViewFieldEnum.UNIVARIATE_STATISTICS_STDDEVPA.GetName(),
                baseStatisticsBean.XStandardDeviationPop);
            result.Put(ViewFieldEnum.UNIVARIATE_STATISTICS_VARIANCE.GetName(), baseStatisticsBean.XVariance);
            result.Put(ViewFieldEnum.UNIVARIATE_STATISTICS_AVERAGE.GetName(), baseStatisticsBean.XAverage);
            additionalProps?.AddProperties(result, lastNewValues);

            return eventAdapterService.AdapterForTypedMap(result, eventType);
        }

        public static EventType CreateEventType(
            StatViewAdditionalPropsForge additionalProps,
            ViewForgeEnv env,
            int streamNum)
        {
            LinkedHashMap<string, object> eventTypeMap = new LinkedHashMap<string, object>();
            eventTypeMap.Put(ViewFieldEnum.UNIVARIATE_STATISTICS_DATAPOINTS.GetName(), typeof(long?));
            eventTypeMap.Put(ViewFieldEnum.UNIVARIATE_STATISTICS_TOTAL.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.UNIVARIATE_STATISTICS_STDDEV.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.UNIVARIATE_STATISTICS_STDDEVPA.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.UNIVARIATE_STATISTICS_VARIANCE.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.UNIVARIATE_STATISTICS_AVERAGE.GetName(), typeof(double?));
            StatViewAdditionalPropsForge.AddCheckDupProperties(
                eventTypeMap,
                additionalProps,
                ViewFieldEnum.UNIVARIATE_STATISTICS_DATAPOINTS,
                ViewFieldEnum.UNIVARIATE_STATISTICS_TOTAL,
                ViewFieldEnum.UNIVARIATE_STATISTICS_STDDEV,
                ViewFieldEnum.UNIVARIATE_STATISTICS_STDDEVPA,
                ViewFieldEnum.UNIVARIATE_STATISTICS_VARIANCE,
                ViewFieldEnum.UNIVARIATE_STATISTICS_AVERAGE
            );
            return DerivedViewTypeUtil.NewType("statview", eventTypeMap, env, streamNum);
        }

        public BaseStatisticsBean BaseStatisticsBean {
            get => baseStatisticsBean;
        }

        public object[] LastValuesEventNew {
            get => lastValuesEventNew;
        }

        public void SetLastValuesEventNew(object[] lastValuesEventNew)
        {
            this.lastValuesEventNew = lastValuesEventNew;
        }

        public ViewFactory ViewFactory {
            get => _viewFactory;
        }
    }
} // end of namespace