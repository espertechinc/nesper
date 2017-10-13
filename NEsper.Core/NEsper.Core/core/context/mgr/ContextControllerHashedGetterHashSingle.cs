///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerHashedGetterHashSingle : EventPropertyGetter
    {
        private readonly ExprEvaluator _eval;
        private readonly int _granularity;

        public ContextControllerHashedGetterHashSingle(ExprEvaluator eval, int granularity)
        {
            _eval = eval;
            _granularity = granularity;
        }

        #region EventPropertyGetter Members

        public Object Get(EventBean eventBean)
        {
            var events = new[] {eventBean};
            object code = _eval.Evaluate(new EvaluateParams(events, true, null));

            int value;
            if (code == null)
            {
                value = 0;
            }
            else
            {
                value = code.GetHashCode()%_granularity;
            }

            if (value >= 0)
            {
                return value;
            }
            return -value;
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return false;
        }

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }

        #endregion
    }
}