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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.named
{
    /// <summary>
    /// View for the on-select statement that handles selecting events from a named window.
    /// </summary>
    public class NamedWindowOnSelectView : NamedWindowOnExprBaseView
    {
        private readonly NamedWindowOnSelectViewFactory _parent;
        private readonly ResultSetProcessor _resultSetProcessor;
        private EventBean[] _lastResult;
        private readonly ISet<MultiKey<EventBean>> _oldEvents = new HashSet<MultiKey<EventBean>>();
        private readonly bool _audit;
        private readonly bool _isDelete;
        private readonly TableStateInstance _tableStateInstanceInsertInto;

        public NamedWindowOnSelectView(SubordWMatchExprLookupStrategy lookupStrategy, NamedWindowRootViewInstance rootView, ExprEvaluatorContext exprEvaluatorContext, NamedWindowOnSelectViewFactory parent, ResultSetProcessor resultSetProcessor, bool audit, bool isDelete, TableStateInstance tableStateInstanceInsertInto)
            : base(lookupStrategy, rootView, exprEvaluatorContext)
        {
            _parent = parent;
            _resultSetProcessor = resultSetProcessor;
            _audit = audit;
            _isDelete = isDelete;
            _tableStateInstanceInsertInto = tableStateInstanceInsertInto;
        }

        public override void HandleMatching(EventBean[] triggerEvents, EventBean[] matchingEvents)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QInfraOnAction(OnTriggerType.ON_SELECT, triggerEvents, matchingEvents); }

            EventBean[] newData;

            // clear state from prior results
            _resultSetProcessor.Clear();

            // build join result
            // use linked hash set to retain order of join results for last/first/window to work most intuitively
            var newEvents = BuildJoinResult(triggerEvents, matchingEvents);

            // process matches
            var pair = _resultSetProcessor.ProcessJoinResult(newEvents, _oldEvents, false);
            newData = (pair != null ? pair.First : null);

            if (_parent.IsDistinct)
            {
                newData = EventBeanUtility.GetDistinctByProp(newData, _parent.EventBeanReader);
            }

            if (_tableStateInstanceInsertInto != null)
            {
                if (newData != null)
                {
                    foreach (var aNewData in newData)
                    {
                        if (_audit)
                        {
                            AuditPath.AuditInsertInto(ExprEvaluatorContext.EngineURI, ExprEvaluatorContext.StatementName, aNewData);
                        }
                        _tableStateInstanceInsertInto.AddEventUnadorned(aNewData);
                    }
                }
            }
            else if (_parent.InternalEventRouter != null)
            {
                if (newData != null)
                {
                    foreach (var aNewData in newData)
                    {
                        if (_audit)
                        {
                            AuditPath.AuditInsertInto(ExprEvaluatorContext.EngineURI, ExprEvaluatorContext.StatementName, aNewData);
                        }
                        _parent.InternalEventRouter.Route(aNewData, _parent.StatementHandle, _parent.InternalEventRouteDest, ExprEvaluatorContext, _parent.IsAddToFront);
                    }
                }
            }

            // The on-select listeners receive the events selected
            if ((newData != null) && (newData.Length > 0))
            {
                // And post only if we have listeners/subscribers that need the data
                if (_parent.StatementResultService.IsMakeNatural || _parent.StatementResultService.IsMakeSynthetic)
                {
                    UpdateChildren(newData, null);
                }
            }
            _lastResult = newData;

            // clear state from prior results
            _resultSetProcessor.Clear();

            // Events to delete are indicated via old data
            if (_isDelete)
            {
                RootView.Update(null, matchingEvents);
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraOnAction(); }
        }

        public static ISet<MultiKey<EventBean>> BuildJoinResult(EventBean[] triggerEvents, EventBean[] matchingEvents)
        {
            var events = new LinkedHashSet<MultiKey<EventBean>>();
            for (var i = 0; i < triggerEvents.Length; i++)
            {
                var triggerEvent = triggerEvents[0];
                if (matchingEvents != null)
                {
                    for (var j = 0; j < matchingEvents.Length; j++)
                    {
                        var eventsPerStream = new EventBean[2];
                        eventsPerStream[0] = matchingEvents[j];
                        eventsPerStream[1] = triggerEvent;
                        events.Add(new MultiKey<EventBean>(eventsPerStream));
                    }
                }
            }
            return events;
        }

        public override EventType EventType
        {
            get
            {
                if (_resultSetProcessor != null)
                {
                    return _resultSetProcessor.ResultEventType;
                }
                else
                {
                    return RootView.EventType;
                }
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            if (_lastResult == null)
                return EnumerationHelper.Empty<EventBean>();
            return ((IEnumerable<EventBean>)_lastResult).GetEnumerator();
        }
    }
} // end of namespace
