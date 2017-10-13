///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.rettype;
using com.espertech.esper.epl.expression.core;

using XLR8.CGLib;

namespace com.espertech.esper.epl.expression.dot
{
    public class ExprDotMethodEvalNoDuckWrapArray : ExprDotMethodEvalNoDuck
    {
        public ExprDotMethodEvalNoDuckWrapArray(String statementName, FastMethod method, ExprEvaluator[] parameters)
            : base(statementName, method, parameters)
        {
        }

        public override Object Evaluate(Object target, EvaluateParams evaluateParams)
        {
            var result = base.Evaluate(target, evaluateParams);
            if (result == null || !result.GetType().IsArray) {
                return null;
            }
            return (Array) result; // doesn't need to be wrapped for CLR
        }

        public override EPType TypeInfo
        {
            get { return EPTypeHelper.CollectionOfSingleValue(Method.ReturnType.GetElementType()); }
        }
    }
}
