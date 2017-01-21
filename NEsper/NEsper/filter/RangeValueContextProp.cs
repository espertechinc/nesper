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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.pattern;

namespace com.espertech.esper.filter
{
    [Serializable]
    public class RangeValueContextProp : FilterSpecParamRangeValue {
        [NonSerialized]
        private readonly EventPropertyGetter _getter;

        public RangeValueContextProp(EventPropertyGetter getter) {
            _getter = getter;
        }
    
        public Object GetFilterValue(MatchedEventMap matchedEvents, ExprEvaluatorContext exprEvaluatorContext) {
            if (exprEvaluatorContext.ContextProperties == null) {
                return null;
            }
            Object @object = _getter.Get(exprEvaluatorContext.ContextProperties);
            if (@object == null) {
                return null;
            }
    
            if (@object is String) {
                return @object;
            }

            return @object.AsDouble();
        }
    }
}
