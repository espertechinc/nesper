///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.settings;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprAppDotMethodImpl : ExprNodeBase,
        FilterSpecCompilerAdvIndexDescProvider,
        FilterExprAnalyzerAffectorProvider
    {
        public ExprAppDotMethodImpl(SettingsApplicationDotMethod desc)
        {
            Desc = desc;
        }

        public SettingsApplicationDotMethod Desc { get; }

        public override ExprForge Forge => Desc.Forge;

        public ExprEvaluator ExprEvaluator => Desc.Forge.ExprEvaluator;

        public Type EvaluationType => Desc.Forge.EvaluationType;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public bool IsConstantResult => false;

        public FilterExprAnalyzerAffector GetAffector(bool isOuterJoin)
        {
            return isOuterJoin ? null : Desc.FilterExprAnalyzerAffector;
        }

        public FilterSpecCompilerAdvIndexDesc FilterSpecDesc => Desc.FilterSpecCompilerAdvIndexDesc;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            Desc.Validate(validationContext);
            return null;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(Desc.LhsName);
            writer.Write("(");
            ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceAsList(Desc.Lhs, writer);
            writer.Write(").");
            writer.Write(Desc.DotMethodName);
            writer.Write("(");
            writer.Write(Desc.RhsName);
            writer.Write("(");
            ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceAsList(Desc.Rhs, writer);
            writer.Write("))");
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprAppDotMethodImpl)) {
                return false;
            }

            var other = (ExprAppDotMethodImpl) node;
            if (!Desc.LhsName.Equals(other.Desc.LhsName)) {
                return false;
            }

            if (!Desc.DotMethodName.Equals(other.Desc.DotMethodName)) {
                return false;
            }

            if (!Desc.RhsName.Equals(other.Desc.RhsName)) {
                return false;
            }

            if (!ExprNodeUtilityCompare.DeepEquals(Desc.Lhs, other.Desc.Lhs, false)) {
                return false;
            }

            return ExprNodeUtilityCompare.DeepEquals(Desc.Rhs, other.Desc.Rhs, false);
        }
    }
} // end of namespace