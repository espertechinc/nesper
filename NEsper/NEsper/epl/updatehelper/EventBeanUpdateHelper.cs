///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.updatehelper
{
    public class EventBeanUpdateHelper {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly EventBeanCopyMethod _copyMethod;
        private readonly EventBeanUpdateItem[] _updateItems;
    
        public EventBeanUpdateHelper(EventBeanCopyMethod copyMethod, EventBeanUpdateItem[] updateItems)
        {
            _copyMethod = copyMethod;
            _updateItems = updateItems;
        }
    
        public EventBean UpdateWCopy(EventBean matchingEvent, EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QInfraUpdate(matchingEvent, eventsPerStream, _updateItems.Length, true);
            }
    
            EventBean copy = _copyMethod.Copy(matchingEvent);
            eventsPerStream[0] = copy;
            eventsPerStream[2] = matchingEvent; // initial value
    
            UpdateInternal(eventsPerStream, exprEvaluatorContext, copy);
    
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().AInfraUpdate(copy);
            }
            return copy;
        }
    
        public void UpdateNoCopy(EventBean matchingEvent, EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext) {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QInfraUpdate(matchingEvent, eventsPerStream, _updateItems.Length, false);
            }
    
            UpdateInternal(eventsPerStream, exprEvaluatorContext, matchingEvent);
    
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().AInfraUpdate(matchingEvent);
            }
        }

        public EventBeanUpdateItem[] UpdateItems
        {
            get { return _updateItems; }
        }

        public bool IsRequiresStream2InitialValueEvent
        {
            get { return _copyMethod != null; }
        }

        private void UpdateInternal(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext, EventBean target) 
        {
            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);
            for (int i = 0; i < _updateItems.Length; i++)
            {
                EventBeanUpdateItem updateItem = _updateItems[i];
    
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().QInfraUpdateRHSExpr(i, updateItem);
                }
                Object result = updateItem.Expression.Evaluate(evaluateParams);
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().AInfraUpdateRHSExpr(result);
                }
    
                if (updateItem.OptionalWriter != null) {
                    if (result == null && updateItem.IsNotNullableField) {
                        Log.Warn("Null value returned by expression for assignment to property '" + updateItem.OptionalPropertyName + " is ignored as the property type is not nullable for expression");
                        continue;
                    }
    
                    if (updateItem.OptionalWidener != null) {
                        result = updateItem.OptionalWidener.Invoke(result);
                    }
                    updateItem.OptionalWriter.Write(result, target);
                }
            }
        }
    }
} // end of namespace
