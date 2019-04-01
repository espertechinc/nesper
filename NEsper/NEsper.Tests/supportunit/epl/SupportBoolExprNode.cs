///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.supportunit.epl
{
    [Serializable]
    public class SupportBoolExprNode : ExprNodeBase, ExprEvaluator
    {
        private readonly bool _evaluateResult;
    
        public SupportBoolExprNode(bool evaluateResult)
        {
            _evaluateResult = evaluateResult;
        }

        public override ExprEvaluator ExprEvaluator => this;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            return null;
        }

        public Type ReturnType => typeof (Boolean);

        public override bool IsConstantResult => false;

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return _evaluateResult;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            throw new UnsupportedOperationException("not implemented");
        }
    }
}
