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

namespace com.espertech.esper.epl.expression.funcs
{
    /// <summary>
    /// Represents the EXISTS(property) function in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprPropertyExistsNode : ExprNodeBase, ExprEvaluator
    {
        private ExprIdentNode _identNode;

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Count != 1)
            {
                throw new ExprValidationException("Exists function node must have exactly 1 child node");
            }
    
            if (!(ChildNodes[0] is ExprIdentNode))
            {
                throw new ExprValidationException("Exists function expects an property value expression as the child node");
            }
    
            _identNode = (ExprIdentNode) ChildNodes[0];
            return null;
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
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprPropExists(this);}
            bool exists = _identNode.ExprEvaluatorIdent.EvaluatePropertyExists(
                evaluateParams.EventsPerStream, 
                evaluateParams.IsNewData);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprPropExists(exists);}
            return exists;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("exists(");
            ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            writer.Write(')');
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            return node is ExprPropertyExistsNode;
        }
    }
}
