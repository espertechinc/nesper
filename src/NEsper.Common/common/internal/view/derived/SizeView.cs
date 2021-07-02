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
        private readonly StatViewAdditionalPropsEval _additionalProps;
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly EventType _eventType;
        private readonly SizeViewFactory _sizeViewFactory;
        private EventBean _lastSizeEvent;
        protected internal object[] lastValuesEventNew;
        protected internal long size;

        public SizeView(
            SizeViewFactory sizeViewFactory,
            AgentInstanceContext agentInstanceContext,
            EventType eventType,
            StatViewAdditionalPropsEval additionalProps)
        {
            this._sizeViewFactory = sizeViewFactory;
            this._agentInstanceContext = agentInstanceContext;
            this._eventType = eventType;
            this._additionalProps = additionalProps;
        }

        public override EventType EventType => _eventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            _agentInstanceContext.AuditProvider.View(newData, oldData, _agentInstanceContext, _sizeViewFactory);
            _agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(_sizeViewFactory, newData, oldData);

            var priorSize = size;

            // If we have child views, keep a reference to the old values, so we can update them as old data event.
            EventBean oldDataMap = null;
            if (_lastSizeEvent == null) {
                if (child != null) {
                    IDictionary<string, object> postOldData = new Dictionary<string, object>();
                    postOldData.Put(ViewFieldEnum.SIZE_VIEW_SIZE.GetName(), priorSize);
                    AddProperties(postOldData);
                    oldDataMap =
                        _agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedMap(postOldData, _eventType);
                }
            }

            // add data points to the window
            if (newData != null) {
                size += newData.Length;

                if (_additionalProps != null && newData.Length != 0) {
                    var additionalEvals = _additionalProps.AdditionalEvals;
                    if (lastValuesEventNew == null) {
                        lastValuesEventNew = new object[additionalEvals.Length];
                    }

                    for (var val = 0; val < additionalEvals.Length; val++) {
                        lastValuesEventNew[val] = additionalEvals[val]
                            .Evaluate(
                                new[] {newData[newData.Length - 1]},
                                true,
                                _agentInstanceContext);
                    }
                }
            }

            if (oldData != null) {
                size -= oldData.Length;
            }

            // If there are child views, fireStatementStopped update method
            if (child != null && priorSize != size) {
                IDictionary<string, object> postNewData = new Dictionary<string, object>();
                postNewData.Put(ViewFieldEnum.SIZE_VIEW_SIZE.GetName(), size);
                AddProperties(postNewData);
                EventBean newEvent =
                    _agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedMap(postNewData, _eventType);

                EventBean[] oldEvents;
                if (_lastSizeEvent != null) {
                    oldEvents = new[] {_lastSizeEvent};
                }
                else {
                    oldEvents = new[] {oldDataMap};
                }

                EventBean[] newEvents = {newEvent};

                _agentInstanceContext.InstrumentationProvider.QViewIndicate(_sizeViewFactory, newEvents, oldEvents);
                child.Update(newEvents, oldEvents);
                _agentInstanceContext.InstrumentationProvider.AViewIndicate();

                _lastSizeEvent = newEvent;
            }

            _agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            var current = new Dictionary<string, object>();
            current.Put(ViewFieldEnum.SIZE_VIEW_SIZE.GetName(), size);
            AddProperties(current);
            var eventBean = _agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedMap(current, _eventType);
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
            schemaMap.Put(ViewFieldEnum.SIZE_VIEW_SIZE.GetName(), typeof(long));
            StatViewAdditionalPropsForge.AddCheckDupProperties(
                schemaMap,
                additionalProps,
                ViewFieldEnum.SIZE_VIEW_SIZE);
            return DerivedViewTypeUtil.NewType("sizeview", schemaMap, env, streamNum);
        }

        private void AddProperties(IDictionary<string, object> newDataMap)
        {
            _additionalProps?.AddProperties(newDataMap, lastValuesEventNew);
        }
    }
} // end of namespace