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
    /// <summary>
    /// A placeholder expression for view/pattern object parameters that allow sorting expression values ascending or descending.
    /// </summary>
    [Serializable]
    public class ExprOrderedExpr 
        : ExprNodeBase
        , ExprEvaluator
    {
        private readonly bool _isDescending;
        [NonSerialized] private ExprEvaluator _evaluator;

        /// <summary>Ctor. </summary>
        /// <param name="descending">is true for descending sorts</param>
        public ExprOrderedExpr(bool descending)
        {
            _isDescending = descending;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            if (_isDescending)
            {
                writer.Write(" desc");
            }
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override bool IsConstantResult
        {
            get { return ChildNodes[0].IsConstantResult; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            var other = node as ExprOrderedExpr;
            if (other == null)
            {
                return false;
            }

            return other._isDescending == _isDescending;
        }
    
        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            _evaluator = ChildNodes[0].ExprEvaluator; // always valid
            return null;
        }

        public Type ReturnType
        {
            get { return ChildNodes[0].ExprEvaluator.ReturnType; }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return _evaluator.Evaluate(evaluateParams);
        }

        /// <summary>Returns true for descending sort. </summary>
        /// <value>indicator for ascending or descending sort</value>
        public bool IsDescending
        {
            get { return _isDescending; }
        }
    }
}
