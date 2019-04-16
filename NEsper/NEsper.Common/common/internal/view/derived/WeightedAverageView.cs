///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.derived
{
    /// <summary>
    ///     View for computing a weighted average. The view uses 2 fields within the parent view to compute the weighted
    ///     average.
    ///     The X field and weight field. In a price-volume example it calculates the volume-weighted average price
    ///     as   (sum(price * volume) / sum(volume)).
    ///     Example: weighted_avg("price", "volume")
    /// </summary>
    public class WeightedAverageView : ViewSupport,
        DerivedValueView
    {
        private readonly AgentInstanceContext agentInstanceContext;

        private readonly EventBean[] eventsPerStream = new EventBean[1];
        private readonly WeightedAverageViewFactory viewFactory;
        protected internal double currentValue = double.NaN;

        private EventBean lastNewEvent;
        protected internal object[] lastValuesEventNew;
        protected internal double sumW = double.NaN;

        protected internal double sumXtimesW = double.NaN;

        public WeightedAverageView(
            WeightedAverageViewFactory viewFactory,
            AgentInstanceViewFactoryChainContext agentInstanceContext)
        {
            this.viewFactory = viewFactory;
            this.agentInstanceContext = agentInstanceContext.AgentInstanceContext;
        }

        public double SumXtimesW {
            get => sumXtimesW;
            set => sumXtimesW = value;
        }

        public double SumW {
            get => sumW;
            set => sumW = value;
        }

        public double CurrentValue {
            get => currentValue;
            set => currentValue = value;
        }

        public object[] LastValuesEventNew {
            get => lastValuesEventNew;
            set => lastValuesEventNew = value;
        }

        public ViewFactory ViewFactory => viewFactory;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, viewFactory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(viewFactory, newData, oldData);

            var oldValue = currentValue;

            // If we have child views, keep a reference to the old values, so we can update them as old data event.
            EventBean oldDataMap = null;
            if (lastNewEvent == null) {
                if (child != null) {
                    IDictionary<string, object> oldDataValues = new Dictionary<string, object>();
                    oldDataValues.Put(ViewFieldEnum.WEIGHTED_AVERAGE__AVERAGE.GetName(), oldValue);
                    AddProperties(oldDataValues);
                    oldDataMap = agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedMap(
                        oldDataValues, viewFactory.eventType);
                }
            }

            // add data points to the bean
            if (newData != null) {
                for (var i = 0; i < newData.Length; i++) {
                    eventsPerStream[0] = newData[i];
                    var pointnum = viewFactory.fieldNameXEvaluator.Evaluate(
                        eventsPerStream, true, agentInstanceContext);
                    var weightnum = viewFactory.fieldNameWeightEvaluator.Evaluate(
                        eventsPerStream, true, agentInstanceContext);
                    if (pointnum != null && weightnum != null) {
                        double point = pointnum.AsDouble();
                        double weight = weightnum.AsDouble();

                        if (Double.IsNaN(sumXtimesW.AsDouble())) {
                            sumXtimesW = point * weight;
                            sumW = weight;
                        }
                        else {
                            sumXtimesW += point * weight;
                            sumW += weight;
                        }
                    }
                }

                if (viewFactory.additionalProps != null && newData.Length != 0) {
                    var additionalEvals = viewFactory.additionalProps.GetAdditionalEvals();
                    if (lastValuesEventNew == null) {
                        lastValuesEventNew = new object[additionalEvals.Length];
                    }

                    for (var val = 0; val < additionalEvals.Length; val++) {
                        lastValuesEventNew[val] = additionalEvals[val]
                            .Evaluate(eventsPerStream, true, agentInstanceContext);
                    }
                }
            }

            // remove data points from the bean
            if (oldData != null) {
                for (var i = 0; i < oldData.Length; i++) {
                    eventsPerStream[0] = oldData[i];
                    var pointnum = viewFactory.fieldNameXEvaluator.Evaluate(
                        eventsPerStream, true, agentInstanceContext);
                    var weightnum = viewFactory.fieldNameWeightEvaluator.Evaluate(
                        eventsPerStream, true, agentInstanceContext);

                    if (pointnum != null && weightnum != null) {
                        double point = pointnum.AsDouble();
                        double weight = weightnum.AsDouble();
                        sumXtimesW -= point * weight;
                        sumW -= weight;
                    }
                }
            }

            if (sumW != 0) {
                currentValue = sumXtimesW / sumW;
            }
            else {
                currentValue = double.NaN;
            }

            // If there are child view, fireStatementStopped update method
            if (child != null) {
                IDictionary<string, object> newDataMap = new Dictionary<string, object>();
                newDataMap.Put(ViewFieldEnum.WEIGHTED_AVERAGE__AVERAGE.GetName(), currentValue);
                AddProperties(newDataMap);
                EventBean newDataEvent =
                    agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedMap(
                        newDataMap, viewFactory.eventType);

                EventBean[] newEvents = {newDataEvent};
                EventBean[] oldEvents;
                if (lastNewEvent == null) {
                    oldEvents = new[] {oldDataMap};
                }
                else {
                    oldEvents = new[] {lastNewEvent};
                }

                agentInstanceContext.InstrumentationProvider.QViewIndicate(viewFactory, newEvents, oldEvents);
                child.Update(newEvents, oldEvents);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();

                lastNewEvent = newDataEvent;
            }

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override EventType EventType => viewFactory.eventType;

        public override IEnumerator<EventBean> GetEnumerator()
        {
            IDictionary<string, object> newDataMap = new Dictionary<string, object>();
            newDataMap.Put(ViewFieldEnum.WEIGHTED_AVERAGE__AVERAGE.GetName(), currentValue);
            AddProperties(newDataMap);
            return EnumerationHelper.SingletonNullable(
                agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedMap(newDataMap, viewFactory.eventType));
        }

        private void AddProperties(IDictionary<string, object> newDataMap)
        {
            if (viewFactory.additionalProps == null) {
                return;
            }

            viewFactory.additionalProps.AddProperties(newDataMap, lastValuesEventNew);
        }

        public static EventType CreateEventType(
            StatViewAdditionalPropsForge additionalProps,
            ViewForgeEnv env,
            int streamNum)
        {
            var schemaMap = new LinkedHashMap<string, object>();
            schemaMap.Put(ViewFieldEnum.WEIGHTED_AVERAGE__AVERAGE.GetName(), typeof(double?));
            StatViewAdditionalPropsForge.AddCheckDupProperties(
                schemaMap, additionalProps, ViewFieldEnum.WEIGHTED_AVERAGE__AVERAGE);
            return DerivedViewTypeUtil.NewType("wavgview", schemaMap, env, streamNum);
        }
    }
} // end of namespace