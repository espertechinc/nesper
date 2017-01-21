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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalInsertWildcardSSWrapper
        : EvalBaseMap
        , SelectExprProcessor
    {
        public EvalInsertWildcardSSWrapper(SelectExprContext selectExprContext,
                                           EventType resultEventType)
            : base(selectExprContext, resultEventType)
        {
        }

        // In case of a wildcard and single stream that is itself a
        // wrapper bean, we also need to add the map properties
        public override EventBean ProcessSpecific(IDictionary<String, Object> props,
                                                  EventBean[] eventsPerStream,
                                                  bool isNewData,
                                                  bool isSynthesize,
                                                  ExprEvaluatorContext exprEvaluatorContext)
        {
            var wrapper = (DecoratingEventBean) eventsPerStream[0];
            if (wrapper != null)
            {
                IDictionary<String, Object> map = wrapper.DecoratingProperties;
                if ((ExprNodes.Length == 0) && (map.IsNotEmpty()))
                {
                    props = new Dictionary<String, Object>(map);
                }
                else
                {
                    props.PutAll(map);
                }
            }

            EventBean theEvent = eventsPerStream[0];

            // Using a wrapper bean since we cannot use the same event type else same-type filters match.
            // Wrapping it even when not adding properties is very inexpensive.
            return EventAdapterService.AdapterForTypedWrapper(theEvent, props, ResultEventType);
        }
    }
}