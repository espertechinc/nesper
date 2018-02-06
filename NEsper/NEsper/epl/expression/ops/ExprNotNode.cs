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
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.ops
{
    /// <summary>
    /// Represents a NOT expression in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprNotNode : ExprNodeBase, ExprEvaluator
    {
        [NonSerialized] private ExprEvaluator _evaluator;
        
        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // Must have a single child node
            if (ChildNodes.Count != 1)
            {
                throw new ExprValidationException("The NOT node requires exactly 1 child node");
            }
    
            _evaluator = ChildNodes[0].ExprEvaluator;
            Type childType = _evaluator.ReturnType;
            if (!childType.IsBoolean())
            {
                throw new ExprValidationException("Incorrect use of NOT clause, sub-expressions do not return bool");
            }

            return null;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public Type ReturnType
        {
            get { return typeof (bool?); }
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprNot(this);}
            var evaluated = (bool?) _evaluator.Evaluate(evaluateParams);
            if (evaluated == null)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprNot(null);}
                return null;
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprNot(!evaluated);}
            return !evaluated;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("not ");
            ChildNodes[0].ToEPL(writer, Precedence);
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.NEGATED; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            return node is ExprNotNode;
        }
    }
}
