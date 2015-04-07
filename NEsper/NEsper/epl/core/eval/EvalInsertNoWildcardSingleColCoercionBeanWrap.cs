///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.client;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalInsertNoWildcardSingleColCoercionBeanWrap
        : EvalBaseFirstProp,
          SelectExprProcessor
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public EvalInsertNoWildcardSingleColCoercionBeanWrap(SelectExprContext selectExprContext,
                                                             EventType resultEventType)
            : base(selectExprContext, resultEventType)
        {
        }

        public override EventBean ProcessFirstCol(Object result)
        {
            EventBean wrappedEvent = base.EventAdapterService.AdapterForObject(result);
            return base.EventAdapterService.AdapterForTypedWrapper(wrappedEvent, new Dictionary<string, object>(),
                                                                   base.ResultEventType);
        }
    }
}