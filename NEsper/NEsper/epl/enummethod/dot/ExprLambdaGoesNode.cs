///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.enummethod.dot
{
    /// <summary>
    /// Represents the case-when-then-else control flow function is an expression tree.
    /// </summary>
    [Serializable]
    public class ExprLambdaGoesNode 
        : ExprNodeBase
        , ExprEvaluator
        , ExprDeclaredOrLambdaNode
    {
        private readonly IList<string> _goesToNames;

        public ExprLambdaGoesNode(IList<string> goesToNames)
        {
            this._goesToNames = goesToNames;
        }

        public bool IsValidated
        {
            get { return true; }
        }

        public IList<string> GoesToNames
        {
            get { return _goesToNames; }
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext) {
            throw new UnsupportedOperationException();
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public Type ReturnType
        {
            get { return null; }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            throw new UnsupportedOperationException();
        }

        public Object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new UnsupportedOperationException();
        }
    
        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix) {
            return false;
        }
    
        public override void ToPrecedenceFreeEPL(TextWriter writer) {
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.MINIMUM; }
        }
    }
    
    
} // end of namespace
