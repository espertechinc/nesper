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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events.vaevent;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalInsertNoWildcardRevision
        : EvalBaseMap
        , SelectExprProcessor
    {
        private readonly ValueAddEventProcessor _vaeProcessor;
        private readonly EventType _vaeInnerEventType;

        public EvalInsertNoWildcardRevision(SelectExprContext selectExprContext, EventType resultEventType, ValueAddEventProcessor vaeProcessor, EventType vaeInnerEventType)
            : base(selectExprContext, resultEventType)
        {
            _vaeProcessor = vaeProcessor;
            _vaeInnerEventType = vaeInnerEventType;
        }

        public override EventBean ProcessSpecific(IDictionary<String, Object> props, EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
        {
            return _vaeProcessor.GetValueAddEventBean(base.EventAdapterService.AdapterForTypedMap(props, _vaeInnerEventType));
        }
    }
}
