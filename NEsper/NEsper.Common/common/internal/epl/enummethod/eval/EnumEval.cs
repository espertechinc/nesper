///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public interface EnumEval
    {
        object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context);
    }

    public delegate object EnumEvalFunc(
        EventBean[] eventsLambda,
        ICollection<object> enumcoll,
        bool isNewData,
        ExprEvaluatorContext context);

    public class ProxyEnumEval : EnumEval
    {
        public EnumEvalFunc ProcEvaluateEnumMethod { get; set; }

        public ProxyEnumEval(EnumEvalFunc procEvaluateEnumMethod)
        {
            ProcEvaluateEnumMethod = procEvaluateEnumMethod;
        }

        public ProxyEnumEval()
        {
        }

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return ProcEvaluateEnumMethod.Invoke(eventsLambda, enumcoll, isNewData, context);
        }
    }
} // end of namespace