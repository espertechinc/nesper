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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalSelectStreamNoUndWEventBeanToObj
        : EvalSelectStreamBaseMap
        , SelectExprProcessor
    {
        private readonly ICollection<String> _eventBeanToObjectProps;

        public EvalSelectStreamNoUndWEventBeanToObj(SelectExprContext selectExprContext,
                                                    EventType resultEventType,
                                                    IList<SelectClauseStreamCompiledSpec> namedStreams,
                                                    Boolean usingWildcard,
                                                    ICollection<String> eventBeanToObjectProps)
            : base(selectExprContext, resultEventType, namedStreams, usingWildcard)
        {
            _eventBeanToObjectProps = eventBeanToObjectProps;
        }

        public override EventBean ProcessSpecific(IDictionary<String, Object> props, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            foreach (String property in _eventBeanToObjectProps) {
                Object value = props.Get(property);
                if (value is EventBean) {
                    props.Put(property, ((EventBean) value).Underlying);
                }
            }
            return SelectExprContext.EventAdapterService.AdapterForTypedMap(props, ResultEventType);
        }
    }
}
