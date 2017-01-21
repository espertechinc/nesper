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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.enummethod.dot
{
    public class ExprDotEvalUnpackCollEventBeanTable : ExprDotEval
    {
        private readonly EPType _typeInfo;
        private readonly TableMetadata _tableMetadata;

        public ExprDotEvalUnpackCollEventBeanTable(EventType type, TableMetadata tableMetadata)
        {
            this._typeInfo = EPTypeHelper.CollectionOfSingleValue(tableMetadata.PublicEventType.UnderlyingType);
            this._tableMetadata = tableMetadata;
        }

        public object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (target == null)
            {
                return null;
            }
            var events = (ICollection<EventBean>) target;
            var underlyings = new ArrayDeque<object>(events.Count);
            foreach (var @event in events)
            {
                underlyings.Add(
                    _tableMetadata.EventToPublic.ConvertToUnd(@event, eventsPerStream, isNewData, exprEvaluatorContext));
            }
            return underlyings;
        }

        public EPType TypeInfo
        {
            get { return _typeInfo; }
        }

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitUnderlyingEventColl();
        }
    }
}
