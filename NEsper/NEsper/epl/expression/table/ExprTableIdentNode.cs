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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.table
{
    [Serializable]
    public class ExprTableIdentNode : ExprNodeBase
    {
        private readonly string _streamOrPropertyName;
        private readonly string _unresolvedPropertyName;
        
        [NonSerialized]
        private ExprEvaluator _eval;
    
        public ExprTableIdentNode(string streamOrPropertyName, string unresolvedPropertyName)
        {
            _streamOrPropertyName = streamOrPropertyName;
            _unresolvedPropertyName = unresolvedPropertyName;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer) {
            ExprIdentNodeImpl.ToPrecedenceFreeEPL(writer, _streamOrPropertyName, _unresolvedPropertyName);
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return _eval; }
        }

        public ExprEvaluator Eval
        {
            set { _eval = value; }
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix) {
            return false;
        }
    
        public override ExprNode Validate(ExprValidationContext validationContext) {
            return null;
        }
    }
}
