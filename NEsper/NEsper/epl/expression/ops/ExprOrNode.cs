///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Linq;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.ops
{
    /// <summary>Represents an OR expression in a filter expression tree. </summary>
    [Serializable]
    public class ExprOrNode
        : ExprNodeBase
        , ExprEvaluator
    {
        [NonSerialized] private ExprEvaluator[] _evaluators;

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            _evaluators = ExprNodeUtility.GetEvaluators(ChildNodes);
    
            // Sub-nodes must be returning bool
            if (_evaluators.Select(child => child.ReturnType).Any(childType => !childType.IsBoolean()))
            {
                throw new ExprValidationException("Incorrect use of OR clause, sub-expressions do not return bool");
            }
    
            if (ChildNodes.Count <= 1)
            {
                throw new ExprValidationException("The OR operator requires at least 2 child expressions");
            }

            return null;
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
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprOr(this);}
            bool? result = false;
            // At least one child must evaluate to true
            foreach (var child in _evaluators)
            {
                var evaluated = (bool?) child.Evaluate(evaluateParams);
                if (evaluated == null)
                {
                    result = null;
                }
                else
                {
                    if (evaluated.Value)
                    {
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprOr(true);}
                        return true;
                    }
                }
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprOr(result);}
            return result;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            String appendStr = "";
            foreach (ExprNode child in ChildNodes)
            {
                writer.Write(appendStr);
                child.ToEPL(writer, Precedence);
                appendStr = " or ";
            }
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.OR; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            return node is ExprOrNode;
        }
    }
}
