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
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalSelectWildcardJoin
        : EvalBaseMap
        , SelectExprProcessor
    {
        private readonly SelectExprProcessor _joinWildcardProcessor;

        public EvalSelectWildcardJoin(SelectExprContext selectExprContext,
                                      EventType resultEventType,
                                      SelectExprProcessor joinWildcardProcessor)
            : base(selectExprContext, resultEventType)
        {
            _joinWildcardProcessor = joinWildcardProcessor;
        }

        public override EventBean ProcessSpecific(IDictionary<String, Object> props,
                                                  EventBean[] eventsPerStream,
                                                  bool isNewData,
                                                  bool isSynthesize,
                                                  ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean theEvent = _joinWildcardProcessor.Process(eventsPerStream, isNewData, isSynthesize,
                                                              exprEvaluatorContext);

            // Using a wrapper bean since we cannot use the same event type else same-type filters match.
            // Wrapping it even when not adding properties is very inexpensive.
            return EventAdapterService.AdapterForTypedWrapper(theEvent, props, ResultEventType);
        }
    }
}