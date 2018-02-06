///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.client;

namespace com.espertech.esper.epl.expression.core
{
    /// <summary>
    /// Represents an expression node that returns the predefined type and
    /// that cannot be evaluated.
    /// </summary>
    [Serializable]public class ExprTypedNoEvalNode 
        : ExprNodeBase 
        , ExprEvaluator
    {
        private readonly string _returnTypeName;
        private readonly Type _returnType;
    
        public ExprTypedNoEvalNode(string returnTypeName, Type returnType)
        {
            _returnTypeName = returnTypeName;
            _returnType = returnType;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext) 
        {
            return null;
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public Type ReturnType
        {
            get { return _returnType; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(_returnTypeName);
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            return false;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            throw new EPException(ReturnType.Name + " cannot be evaluated");
        }
    }
}
