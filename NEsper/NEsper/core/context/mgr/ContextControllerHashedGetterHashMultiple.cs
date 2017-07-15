///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerHashedGetterHashMultiple : EventPropertyGetter
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ExprEvaluator[] _evaluators;
        private readonly int _granularity;

        public ContextControllerHashedGetterHashMultiple(IList<ExprNode> nodes, int granularity)
        {
            _evaluators = new ExprEvaluator[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                _evaluators[i] = nodes[i].ExprEvaluator;
            }
            _granularity = granularity;
        }

        public Object Get(EventBean eventBean)
        {
            var events = new EventBean[]
            {
                eventBean
            };
            var evaluateParams = new EvaluateParams(events, true, null);

            int hashCode = 0;
            for (int i = 0; i < _evaluators.Length; i++)
            {
                object result = _evaluators[i].Evaluate(evaluateParams);
                if (result == null)
                {
                    continue;
                }
                if (hashCode == 0)
                {
                    hashCode = result.GetHashCode();
                }
                else
                {
                    hashCode = 31*hashCode + result.GetHashCode();
                }
            }

            if (hashCode >= 0)
            {
                return hashCode%_granularity;
            }
            return -hashCode%_granularity;
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
} // end of namespace