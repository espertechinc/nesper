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

namespace com.espertech.esper.epl.expression.time
{
    /// <summary>
    /// Represents the CURRENT_TIMESTAMP() function or reserved keyword in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprTimestampNode : ExprNodeBase, ExprEvaluator
    {
        /// <summary>Ctor. </summary>
        public ExprTimestampNode()
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
                throw new ExprValidationException("current_timestamp function node cannot have a child node");
            }

            return null;
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public Type ReturnType
        {
            get { return typeof (long?); }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED) {
                var value = evaluateParams.ExprEvaluatorContext.TimeProvider.Time;
                InstrumentationHelper.Get().QaExprTimestamp(this, value);
                return value;
            }
            return evaluateParams.ExprEvaluatorContext.TimeProvider.Time;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("current_timestamp()");
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            return node is ExprTimestampNode;
        }
    }
}
