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
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.events.vaevent;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalInsertWildcardSSWrapperRevision : EvalBaseMap, SelectExprProcessor {
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly ValueAddEventProcessor _vaeProcessor;
    
        public EvalInsertWildcardSSWrapperRevision(SelectExprContext selectExprContext, EventType resultEventType, ValueAddEventProcessor vaeProcessor)
            : base(selectExprContext, resultEventType)
        {
            _vaeProcessor = vaeProcessor;
        }
    
        // In case of a wildcard and single stream that is itself a
        // wrapper bean, we also need to add the map properties
        public override EventBean ProcessSpecific(
            IDictionary<string, Object> props,
            EventBean[] eventsPerStream,
            bool isNewData,
            bool isSynthesize,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var wrapper = (DecoratingEventBean) eventsPerStream[0];
            if (wrapper != null)
            {
                IDictionary<string, Object> map = wrapper.DecoratingProperties;
                if ((base.ExprNodes.Length == 0) && (!map.IsEmpty()))
                {
                    // no action
                }
                else
                {
                    props.PutAll(map);
                }
            }

            EventBean theEvent = eventsPerStream[0];
            return _vaeProcessor.GetValueAddEventBean(theEvent);
        }
    }
} // end of namespace
