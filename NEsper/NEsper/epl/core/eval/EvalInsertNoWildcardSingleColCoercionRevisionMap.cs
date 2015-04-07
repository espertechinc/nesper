///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.events.vaevent;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalInsertNoWildcardSingleColCoercionRevisionMap : EvalBaseFirstProp, SelectExprProcessor {
        private readonly ValueAddEventProcessor _vaeProcessor;
        private readonly EventType _vaeInnerEventType;
    
        public EvalInsertNoWildcardSingleColCoercionRevisionMap(SelectExprContext selectExprContext, EventType resultEventType, ValueAddEventProcessor vaeProcessor, EventType vaeInnerEventType)
                    : base(selectExprContext, resultEventType)
        {
            _vaeProcessor = vaeProcessor;
            _vaeInnerEventType = vaeInnerEventType;
        }
    
        public override EventBean ProcessFirstCol(Object result) {
            var wrappedEvent = EventAdapterService.AdapterForTypedMap((IDictionary<string, object>) result, ResultEventType);
            return _vaeProcessor.GetValueAddEventBean(
                EventAdapterService.AdapterForTypedWrapper(wrappedEvent, new Dictionary <string, object>(), _vaeInnerEventType));
        }
    }
}
