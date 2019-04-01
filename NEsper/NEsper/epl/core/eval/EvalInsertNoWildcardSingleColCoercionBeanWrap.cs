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
using com.espertech.esper.events;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalInsertNoWildcardSingleColCoercionBeanWrap : EvalBaseFirstPropFromWrap, SelectExprProcessor
    {
        public EvalInsertNoWildcardSingleColCoercionBeanWrap(SelectExprContext selectExprContext, WrapperEventType wrapper)
            : base(selectExprContext, wrapper)
        {
        }
    
        public override EventBean ProcessFirstCol(Object result) {
            EventBean wrappedEvent = base.EventAdapterService.AdapterForTypedObject(result, Wrapper.UnderlyingEventType);
            return base.EventAdapterService.AdapterForTypedWrapper(wrappedEvent, Collections.EmptyDataMap, Wrapper);
        }
    }
} // end of namespace
