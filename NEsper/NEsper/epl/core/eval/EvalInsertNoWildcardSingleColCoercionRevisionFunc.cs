///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.events;
using com.espertech.esper.events.vaevent;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalInsertNoWildcardSingleColCoercionRevisionFunc 
        : EvalBaseFirstProp
        , SelectExprProcessor
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly ValueAddEventProcessor _vaeProcessor;
        private readonly EventType _vaeInnerEventType;
        private readonly Func<EventAdapterService, object, EventType, EventBean> _func;

        public EvalInsertNoWildcardSingleColCoercionRevisionFunc(
            SelectExprContext selectExprContext,
            EventType resultEventType,
            ValueAddEventProcessor vaeProcessor,
            EventType vaeInnerEventType,
            Func<EventAdapterService, object, EventType, EventBean> func)
            : base(selectExprContext, resultEventType)
        {
            _vaeProcessor = vaeProcessor;
            _vaeInnerEventType = vaeInnerEventType;
            _func = func;
        }
    
        public override EventBean ProcessFirstCol(Object result)
        {
            EventBean wrappedEvent = _func.Invoke(base.EventAdapterService, result, base.ResultEventType);
            return _vaeProcessor.GetValueAddEventBean(base.EventAdapterService.AdapterForTypedWrapper(wrappedEvent, Collections.EmptyDataMap, _vaeInnerEventType));
        }
    }
} // end of namespace
