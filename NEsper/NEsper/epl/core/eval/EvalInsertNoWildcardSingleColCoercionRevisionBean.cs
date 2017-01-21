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
using com.espertech.esper.compat.logging;
using com.espertech.esper.events.vaevent;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalInsertNoWildcardSingleColCoercionRevisionBean
        : EvalBaseFirstProp,
          SelectExprProcessor
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EventType _vaeInnerEventType;
        private readonly ValueAddEventProcessor _vaeProcessor;

        public EvalInsertNoWildcardSingleColCoercionRevisionBean(SelectExprContext selectExprContext,
                                                                 EventType resultEventType,
                                                                 ValueAddEventProcessor vaeProcessor,
                                                                 EventType vaeInnerEventType)
            : base(selectExprContext, resultEventType)
        {
            _vaeProcessor = vaeProcessor;
            _vaeInnerEventType = vaeInnerEventType;
        }

        public override EventBean ProcessFirstCol(Object result)
        {
            return
                _vaeProcessor.GetValueAddEventBean(base.EventAdapterService.AdapterForTypedObject(result, _vaeInnerEventType));
        }
    }
}