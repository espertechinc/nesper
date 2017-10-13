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
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.table.onaction
{
    public class TableOnSelectView : TableOnViewBase
    {
        private readonly TableOnSelectViewFactory _parent;
        private readonly ResultSetProcessor _resultSetProcessor;
        private readonly bool _audit;
        private readonly bool _deleteAndSelect;

        public TableOnSelectView(SubordWMatchExprLookupStrategy lookupStrategy, TableStateInstance rootView, ExprEvaluatorContext exprEvaluatorContext, TableMetadata metadata,
                                 TableOnSelectViewFactory parent, ResultSetProcessor resultSetProcessor, bool audit, bool deleteAndSelect)
            : base(lookupStrategy, rootView, exprEvaluatorContext, metadata, deleteAndSelect)
        {
            _parent = parent;
            _resultSetProcessor = resultSetProcessor;
            _audit = audit;
            _deleteAndSelect = deleteAndSelect;
        }

        public override void HandleMatching(EventBean[] triggerEvents, EventBean[] matchingEvents)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QInfraOnAction(OnTriggerType.ON_SELECT, triggerEvents, matchingEvents); }

            EventBean[] newData;

            // clear state from prior results
            _resultSetProcessor.Clear();

            // build join result
            // use linked hash set to retain order of join results for last/first/window to work most intuitively
            ISet<MultiKey<EventBean>> newEvents = NamedWindowOnSelectView.BuildJoinResult(triggerEvents, matchingEvents);

            // process matches
            UniformPair<EventBean[]> pair = _resultSetProcessor.ProcessJoinResult(newEvents, Collections.GetEmptySet<MultiKey<EventBean>>(), false);
            newData = (pair != null ? pair.First : null);

            if (_parent.IsDistinct)
            {
                newData = EventBeanUtility.GetDistinctByProp(newData, _parent.EventBeanReader);
            }

            if (_parent.InternalEventRouter != null)
            {
                if (newData != null)
                {
                    for (int i = 0; i < newData.Length; i++)
                    {
                        if (_audit)
                        {
                            AuditPath.AuditInsertInto(ExprEvaluatorContext.EngineURI, ExprEvaluatorContext.StatementName, newData[i]);
                        }
                        _parent.InternalEventRouter.Route(newData[i], _parent.StatementHandle, _parent.InternalEventRouteDest, ExprEvaluatorContext, false);
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

            // clear state from prior results
            _resultSetProcessor.Clear();

            // Events to delete are indicated via old data
            if (_deleteAndSelect)
            {
                foreach (EventBean @event in matchingEvents)
                {
                    TableStateInstance.DeleteEvent(@event);
                }
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraOnAction(); }
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
                    return base.EventType;
                }
            }
        }
    }
}
