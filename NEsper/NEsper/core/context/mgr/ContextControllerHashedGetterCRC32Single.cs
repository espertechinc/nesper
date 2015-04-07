///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerHashedGetterCRC32Single : EventPropertyGetter
    {
        private readonly ExprEvaluator _eval;
        private readonly int _granularity;
    
        public ContextControllerHashedGetterCRC32Single(ExprEvaluator eval, int granularity) {
            _eval = eval;
            _granularity = granularity;
        }
    
        public Object Get(EventBean eventBean)
        {
            var events = new[] {eventBean};
            var code = (String) _eval.Evaluate(new EvaluateParams(events, true, null));
    
            long value;
            if (code == null)
            {
                value = 0;
            }
            else
            {
                value = code.GetCrc32() % _granularity;
            }
    
            var result = (int) value;
            if (result >= 0) {
                return result;
            }
            return -result;
        }
    
        public bool IsExistsProperty(EventBean eventBean)
        {
            return false;
        }
    
        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }
    }
}
