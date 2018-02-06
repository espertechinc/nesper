///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.epl.expression.core
{
    [Serializable]
    public class ExprGroupingIdNode : ExprNodeBase, ExprEvaluator
    {
        private int _id;

        public int Id
        {
            set { _id = value; }
            get { return _id; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (!validationContext.IsAllowRollupFunctions)
            {
                throw MakeException("grouping_id");
            }

            return null;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ExprNodeUtility.ToExpressionStringWFunctionName("grouping_id", ChildNodes, writer);
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            return false;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return _id;
        }

        public Type ReturnType
        {
            get { return typeof (int?); }
        }

        public static ExprValidationException MakeException(String functionName)
        {
            return new ExprValidationException("The " + functionName + " function requires the group-by clause to specify rollup, cube or grouping sets, and may only be used in the select-clause, having-clause or order-by-clause");
        }
    }
}
