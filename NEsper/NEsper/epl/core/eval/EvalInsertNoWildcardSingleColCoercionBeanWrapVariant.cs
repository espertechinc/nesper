///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.events.vaevent;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalInsertNoWildcardSingleColCoercionBeanWrapVariant
        : EvalBaseFirstProp
        , SelectExprProcessor
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ValueAddEventProcessor _vaeProcessor;

        public EvalInsertNoWildcardSingleColCoercionBeanWrapVariant(
            SelectExprContext selectExprContext,
            EventType resultEventType,
            ValueAddEventProcessor vaeProcessor)
            : base(selectExprContext, resultEventType)
        {
            _vaeProcessor = vaeProcessor;
        }

        public override EventBean ProcessFirstCol(Object result)
        {
            EventBean wrappedEvent = base.EventAdapterService.AdapterForObject(result);
            EventBean variant = _vaeProcessor.GetValueAddEventBean(wrappedEvent);
            return base.EventAdapterService.AdapterForTypedWrapper(
                variant, Collections.EmptyDataMap, base.ResultEventType);
        }
    }
} // end of namespace