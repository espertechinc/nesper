///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.epl.expression.core
{
    [Serializable]
    public class ExprNodeUtilExprEvalStreamNumEnumColl : ExprEvaluator
    {
        private readonly ExprEvaluatorEnumeration _enumeration;

        public ExprNodeUtilExprEvalStreamNumEnumColl(ExprEvaluatorEnumeration enumeration)
        {
            _enumeration = enumeration;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return _enumeration.EvaluateGetROCollectionEvents(evaluateParams);
        }

        public Type ReturnType
        {
            get { return typeof (EventBean); }
        }
    }
}
