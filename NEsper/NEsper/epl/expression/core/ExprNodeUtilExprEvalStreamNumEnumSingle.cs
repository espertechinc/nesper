///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.epl.expression.core
{
    [Serializable]
    public class ExprNodeUtilExprEvalStreamNumEnumSingle : ExprEvaluator
    {
        private readonly ExprEvaluatorEnumeration _enumeration;

        public ExprNodeUtilExprEvalStreamNumEnumSingle(ExprEvaluatorEnumeration enumeration)
        {
            _enumeration = enumeration;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return _enumeration.EvaluateGetEventBean(
                evaluateParams.EventsPerStream,
                evaluateParams.IsNewData,
                evaluateParams.ExprEvaluatorContext
            );
        }

        public Type ReturnType
        {
            get { return typeof (ICollection<object>); }
        }
    }
}
