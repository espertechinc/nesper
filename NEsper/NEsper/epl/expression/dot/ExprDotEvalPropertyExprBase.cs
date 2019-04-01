///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.dot
{
    [Serializable]
    public abstract class ExprDotEvalPropertyExprBase : ExprEvaluator
    {
        protected internal readonly String StatementName;
        protected internal readonly String PropertyName;
        protected internal readonly int StreamNum;
        protected internal readonly ExprEvaluator ExprEvaluator;
        private readonly Type _propertyType;

        protected ExprDotEvalPropertyExprBase(string statementName, string propertyName, int streamNum, ExprEvaluator exprEvaluator, Type propertyType)
        {
            StatementName = statementName;
            PropertyName = propertyName;
            StreamNum = streamNum;
            ExprEvaluator = exprEvaluator;
            _propertyType = propertyType;
        }

        public Type ReturnType
        {
            get { return _propertyType; }
        }

        public abstract object Evaluate(EvaluateParams evaluateParams);

        protected String GetWarningText(String expectedType, Object received)
        {
            return string.Format(
                "Statement '{0}' property {1} parameter expression expected a value of {2} but received {3}",
                StatementName,
                PropertyName,
                expectedType,
                received == null ? "null" : received.GetType().GetCleanName()
            );
        }
    }
}
