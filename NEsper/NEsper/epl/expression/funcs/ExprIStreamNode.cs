///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.expression.funcs
{
    /// <summary>
    /// Represents the RSTREAM() function in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprIStreamNode
        : ExprNodeBase
        , ExprEvaluator
    {
        /// <summary>Ctor. </summary>
        public ExprIStreamNode()
        {
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Count != 0)
            {
                throw new ExprValidationException("current_timestamp function node must have exactly 1 child node");
            }

            return null;
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public Type ReturnType
        {
            get { return typeof (bool); }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QaExprIStream(this, evaluateParams.IsNewData); }
            return evaluateParams.IsNewData;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("istream()");
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            return node is ExprIStreamNode;
        }
    }
}
