///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.epl.expression.visitor;

namespace com.espertech.esper.epl.expression.core
{
    /// <summary>
    /// A placeholder for another expression node that has been validated already.
    /// </summary>
    [Serializable]
    public class ExprNodeValidated 
        : ExprNodeBase
        , ExprEvaluator
    {
        private readonly ExprNode _inner;

        /// <summary>Ctor. </summary>
        /// <param name="inner">nested expression node</param>
        public ExprNodeValidated(ExprNode inner)
        {
            _inner = inner;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return _inner.Precedence; }
        }

        public override void ToEPL(TextWriter writer, ExprPrecedenceEnum parentPrecedence)
        {
            _inner.ToEPL(writer, parentPrecedence);
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            _inner.ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
        }

        public override bool IsConstantResult
        {
            get { return _inner.IsConstantResult; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            if (node is ExprNodeValidated)
            {
                return _inner.EqualsNode(((ExprNodeValidated) node)._inner, false);
            }
            return _inner.EqualsNode(node, false);
        }
    
        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            return null;
        }
    
        public override void Accept(ExprNodeVisitor visitor)
        {
            if (visitor.IsVisit(this))
            {
                visitor.Visit(this);
                _inner.Accept(visitor);
            }
        }

        public Type ReturnType
        {
            get { return _inner.ExprEvaluator.ReturnType; }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return _inner.ExprEvaluator.Evaluate(evaluateParams);
        }
    }
}
