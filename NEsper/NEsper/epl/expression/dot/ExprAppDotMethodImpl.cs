///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.filter;

namespace com.espertech.esper.epl.expression.dot
{
    [Serializable]
    public class ExprAppDotMethodImpl : ExprNodeBase
        , FilterSpecCompilerAdvIndexDescProvider
        , FilterExprAnalyzerAffectorProvider
    {
        private readonly EngineImportApplicationDotMethod _desc;

        public ExprAppDotMethodImpl(EngineImportApplicationDotMethod desc)
        {
            _desc = desc;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            _desc.Validate(validationContext);
            return null;
        }

        public EngineImportApplicationDotMethod Desc => _desc;

        public override ExprEvaluator ExprEvaluator => _desc.ExprEvaluator;

        public FilterSpecCompilerAdvIndexDesc FilterSpecDesc => _desc.GetFilterSpecCompilerAdvIndexDesc();

        public FilterExprAnalyzerAffector GetAffector(bool isOuterJoin)
        {
            return isOuterJoin ? null : _desc.GetFilterExprAnalyzerAffector();
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(_desc.LhsName);
            writer.Write("(");
            ExprNodeUtility.ToExpressionStringMinPrecedenceAsList(_desc.Lhs, writer);
            writer.Write(").");
            writer.Write(_desc.DotMethodName);
            writer.Write("(");
            writer.Write(_desc.RhsName);
            writer.Write("(");
            ExprNodeUtility.ToExpressionStringMinPrecedenceAsList(_desc.Rhs, writer);
            writer.Write("))");
        }

        public override bool IsConstantResult => false;

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            if (node is ExprAppDotMethodImpl other)
            {
                if (!_desc.LhsName.Equals(other.Desc.LhsName)) return false;
                if (!_desc.DotMethodName.Equals(other.Desc.DotMethodName)) return false;
                if (!_desc.RhsName.Equals(other.Desc.RhsName)) return false;
                if (!ExprNodeUtility.DeepEquals(_desc.Lhs, other.Desc.Lhs, false)) return false;
                return ExprNodeUtility.DeepEquals(_desc.Rhs, other.Desc.Rhs, false);
            }

            return false;
        }
    }
} // end of namespace
