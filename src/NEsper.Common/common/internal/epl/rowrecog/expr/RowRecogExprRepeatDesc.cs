///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.rowrecog.expr
{
    [Serializable]
    public class RowRecogExprRepeatDesc
    {
        public RowRecogExprRepeatDesc(
            ExprNode lower,
            ExprNode upper,
            ExprNode single)
        {
            Lower = lower;
            Upper = upper;
            Single = single;
        }

        public ExprNode Lower { get; }

        public ExprNode Upper { get; }

        public ExprNode Single { get; }

        public void ToExpressionString(TextWriter writer)
        {
            writer.Write("{");
            if (Single != null) {
                writer.Write(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(Single));
            }
            else {
                if (Lower != null) {
                    writer.Write(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(Lower));
                }

                writer.Write(",");
                if (Upper != null) {
                    writer.Write(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(Upper));
                }
            }

            writer.Write("}");
        }

        public RowRecogExprRepeatDesc CheckedCopy(ExpressionCopier expressionCopier)
        {
            var lowerCopy = Lower == null ? null : expressionCopier.Copy(Lower);
            var upperCopy = Upper == null ? null : expressionCopier.Copy(Upper);
            var singleCopy = Single == null ? null : expressionCopier.Copy(Single);
            return new RowRecogExprRepeatDesc(lowerCopy, upperCopy, singleCopy);
        }
    }
} // end of namespace