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
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalSelectStreamNoUndWEventBeanToObjObjArray : EvalSelectStreamBaseObjectArray, SelectExprProcessor
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly ISet<int> _eventBeanToObjectIndexes;

        public EvalSelectStreamNoUndWEventBeanToObjObjArray(
            SelectExprContext selectExprContext,
            EventType resultEventType,
            IList<SelectClauseStreamCompiledSpec> namedStreams,
            bool usingWildcard,
            IEnumerable<string> eventBeanToObjectProps)
            : base(selectExprContext, resultEventType, namedStreams, usingWildcard)
        {
            _eventBeanToObjectIndexes = new HashSet<int>();
            var type = (ObjectArrayEventType) resultEventType;
            foreach (var name in eventBeanToObjectProps) {
                int index;
                if (type.PropertiesIndexes.TryGetValue(name, out index)) {
                    _eventBeanToObjectIndexes.Add(index);
                }
            }
        }

        public override EventBean ProcessSpecific(
            Object[] props,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            foreach (var propertyIndex in _eventBeanToObjectIndexes)
            {
                var value = props[propertyIndex];
                if (value is EventBean)
                {
                    props[propertyIndex] = ((EventBean) value).Underlying;
                }
            }
            return base.SelectExprContext.EventAdapterService.AdapterForTypedObjectArray(props, base.ResultEventType);
        }
    }
} // end of namespace
