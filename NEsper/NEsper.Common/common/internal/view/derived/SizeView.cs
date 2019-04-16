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
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.derived
{
    /// <summary>
    ///     This view is a very simple view presenting the number of elements in a stream or view.
    ///     The view computes a single long-typed count of the number of events passed through it similar
    ///     to the base statistics COUNT column.
    /// </summary>
    public class SizeView : ViewSupport
    {
        private readonly StatViewAdditionalPropsEval additionalProps;
        private readonly AgentInstanceContext agentInstanceContext;
        private readonly EventType eventType;
        private readonly SizeViewFactory sizeViewFactory;
        private EventBean lastSizeEvent;
        protected internal object[] lastValuesEventNew;
        protected internal long size;

        public SizeView(
            SizeViewFactory sizeViewFactory,
            AgentInstanceContext agentInstanceContext,
            EventType eventType,
            StatViewAdditionalPropsEval additionalProps)
        {
            this.sizeViewFactory = sizeViewFactory;
            this.agentInstanceContext = agentInstanceContext;
            this.eventType = eventType;
            this.additionalProps = additionalProps;
        }

        public override EventType EventType => eventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, sizeViewFactory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(sizeViewFactory, newData, oldData);

            var priorSize = size;

            // If we have child views, keep a reference to the old values, so we can update them as old data event.
            EventBean oldDataMap = null;
            if (lastSizeEvent == null) {
                if (child != null) {
                    IDictionary<string, object> postOldData = new Dictionary<string, object>();
                    postOldData.Put(ViewFieldEnum.SIZE_VIEW__SIZE.GetName(), priorSize);
                    AddProperties(postOldData);
                    oldDataMap =
                        agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedMap(postOldData, eventType);
                }
            }

            // add data points to the window
            if (newData != null) {
                size += newData.Length;

                if (additionalProps != null && newData.Length != 0) {
                    var additionalEvals = additionalProps.GetAdditionalEvals();
                    if (lastValuesEventNew == null) {
                        lastValuesEventNew = new object[additionalEvals.Length];
                    }

                    for (var val = 0; val < additionalEvals.Length; val++) {
                        lastValuesEventNew[val] = additionalEvals[val].Evaluate(
                            new[] {newData[newData.Length - 1]}, true, agentInstanceContext);
                    }
                }
            }

            if (oldData != null) {
                size -= oldData.Length;
            }

            // If there are child views, fireStatementStopped update method
            if (child != null && priorSize != size) {
                IDictionary<string, object> postNewData = new Dictionary<string, object>();
                postNewData.Put(ViewFieldEnum.SIZE_VIEW__SIZE.GetName(), size);
                AddProperties(postNewData);
                EventBean newEvent =
                    agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedMap(postNewData, eventType);

                EventBean[] oldEvents;
                if (lastSizeEvent != null) {
                    oldEvents = new[] {lastSizeEvent};
                }
                else {
                    oldEvents = new[] {oldDataMap};
                }

                EventBean[] newEvents = {newEvent};

                agentInstanceContext.InstrumentationProvider.QViewIndicate(sizeViewFactory, newEvents, oldEvents);
                child.Update(newEvents, oldEvents);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();

                lastSizeEvent = newEvent;
            }

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            var current = new Dictionary<string, object>();
            current.Put(ViewFieldEnum.SIZE_VIEW__SIZE.GetName(), size);
            AddProperties(current);
            var eventBean = agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedMap(current, eventType);
            if (eventBean != null) {
                yield return eventBean;
            }
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        public static EventType CreateEventType(
            ViewForgeEnv env,
            StatViewAdditionalPropsForge additionalProps,
            int streamNum)
        {
            var schemaMap = new LinkedHashMap<string, object>();
            schemaMap.Put(ViewFieldEnum.SIZE_VIEW__SIZE.GetName(), typeof(long));
            StatViewAdditionalPropsForge.AddCheckDupProperties(
                schemaMap, additionalProps, ViewFieldEnum.SIZE_VIEW__SIZE);
            return DerivedViewTypeUtil.NewType("sizeview", schemaMap, env, streamNum);
        }

        private void AddProperties(IDictionary<string, object> newDataMap)
        {
            if (additionalProps == null) {
                return;
            }

            additionalProps.AddProperties(newDataMap, lastValuesEventNew);
        }
    }
} // end of namespace