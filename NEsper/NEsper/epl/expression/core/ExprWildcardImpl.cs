///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.type;

namespace com.espertech.esper.epl.expression.core
{
    /// <summary>
    /// Expression for use within crontab to specify a wildcard.
    /// </summary>
    [Serializable]
    public class ExprWildcardImpl
        : ExprNodeBase
        , ExprEvaluator
        , ExprWildcard
    {
        private static readonly WildcardParameter wildcardParameter = new WildcardParameter();

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("*");
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
            get { return true; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            return node is ExprWildcardImpl;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            return null;
        }

        public Type ReturnType
        {
            get { return typeof(WildcardParameter); }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return wildcardParameter;
        }
    }
}
