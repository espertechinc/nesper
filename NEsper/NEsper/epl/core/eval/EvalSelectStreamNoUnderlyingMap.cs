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
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalSelectStreamNoUnderlyingMap : EvalSelectStreamBaseMap, SelectExprProcessor {
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public EvalSelectStreamNoUnderlyingMap(
            SelectExprContext selectExprContext,
            EventType resultEventType,
            List<SelectClauseStreamCompiledSpec> namedStreams,
            bool usingWildcard)
            : base(selectExprContext, resultEventType, namedStreams, usingWildcard)
        {
        }

        public override EventBean ProcessSpecific(
            IDictionary<string, Object> props,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return base.SelectExprContext.EventAdapterService.AdapterForTypedMap(props, base.ResultEventType);
        }
    }
} // end of namespace
