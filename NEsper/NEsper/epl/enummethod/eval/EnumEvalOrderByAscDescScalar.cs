///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalOrderByAscDescScalar
        : EnumEvalBase
        , EnumEval
    {
        private readonly bool _descending;

        public EnumEvalOrderByAscDescScalar(int streamCountIncoming, bool descending)
            : base(streamCountIncoming)
        {
            _descending = descending;
        }

        public object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            if (target == null || target.IsEmpty())
            {
                return target;
            }

            var list = new List<object>(target);
            if (_descending)
            {
                list.Sort();
                list.Reverse();
                //Collections.Sort(list, Collections.ReverseOrder());
            }
            else
            {
                list.Sort();
            }
            return list;
        }
    }
}
