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

namespace com.espertech.esper.common.@internal.epl.rowrecog.expr
{
    /// <summary>
    ///     Permute () regular expression in a regex expression tree.
    /// </summary>
    [Serializable]
    public class RowRecogExprNodePermute : RowRecogExprNode
    {
        public override RowRecogExprNodePrecedenceEnum Precedence => RowRecogExprNodePrecedenceEnum.UNARY;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            var delimiter = "";
            writer.Write("match_recognize_permute(");
            foreach (var node in ChildNodes) {
                writer.Write(delimiter);
                node.ToEPL(writer, Precedence);
                delimiter = ", ";
            }

            writer.Write(")");
        }

        public override RowRecogExprNode CheckedCopySelf(ExpressionCopier expressionCopier)
        {
            return new RowRecogExprNodePermute();
        }
    }
} // end of namespace