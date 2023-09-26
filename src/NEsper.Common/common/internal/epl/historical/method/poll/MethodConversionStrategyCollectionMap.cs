///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.historical.method.poll
{
    public class MethodConversionStrategyCollectionMap : MethodConversionStrategyCollection
    {
        protected override EventBean GetEventBean(
            object value,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return exprEvaluatorContext.EventBeanTypedEventFactory.AdapterForTypedMap(
                value.UnwrapStringDictionary(),
                eventType);
        }
    }
} // end of namespace