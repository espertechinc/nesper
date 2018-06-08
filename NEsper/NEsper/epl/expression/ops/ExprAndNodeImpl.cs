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
    /// <summary>Represents an And-condition. </summary>
    [Serializable]
    public class ExprAndNodeImpl : ExprNodeBase, ExprEvaluator, ExprAndNode
    {
        [NonSerialized] private ExprEvaluator[] _evaluators;
    
        public ExprAndNodeImpl()
        {
        }
    
        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            _evaluators = ExprNodeUtility.GetEvaluators(ChildNodes);
    
            // Sub-nodes must be returning bool
            if (_evaluators.Select(child => child.ReturnType).Any(childType => !childType.IsBoolean()))
            {
                throw new ExprValidationException("Incorrect use of AND clause, sub-expressions do not return bool");
            }
    
            if (ChildNodes.Count <= 1)
            {
                throw new ExprValidationException("The AND operator requires at least 2 child expressions");
            }

            return null;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public Type ReturnType
        {
            get { return typeof (bool?); }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprAnd(this); }

            bool? result = true;

            unchecked
            {
                var evaluators = _evaluators;
                var evaluatorsLength = evaluators.Length;

                for (int ii = 0; ii < evaluatorsLength; ii++)
                {
                    var evaluated = evaluators[ii].Evaluate(evaluateParams);
                    if (evaluated == null)
                    {
                        result = null;
                    }
                    else if (false.Equals(evaluated))
                    {
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprAnd(false); }
                        return false;
                    }
                }
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprAnd(result); }

            return result;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            String appendStr = "";
            foreach (ExprNode child in ChildNodes)
            {
                writer.Write(appendStr);
                child.ToEPL(writer, Precedence);
                appendStr = " and ";
            }
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.AND; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            return node is ExprAndNodeImpl;
        }
    }
}
