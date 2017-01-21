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

namespace com.espertech.esper.epl.core.eval
{
    public class EvalInsertNoWildcardSingleColCoercionBean
        : EvalBaseFirstProp,
          SelectExprProcessor
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public EvalInsertNoWildcardSingleColCoercionBean(SelectExprContext selectExprContext,
                                                         EventType resultEventType)
            : base(selectExprContext, resultEventType)
        {
        }

        public override EventBean ProcessFirstCol(Object result)
        {
            return base.EventAdapterService.AdapterForTypedObject(result, base.ResultEventType);
        }
    }
}