///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    ///     Or-condition in a regex expression tree.
    /// </summary>
    public class RowRecogExprNodeAlteration : RowRecogExprNode
    {
        public override RowRecogExprNodePrecedenceEnum Precedence => RowRecogExprNodePrecedenceEnum.ALTERNATION;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            var delimiter = "";
            foreach (var node in ChildNodes) {
                writer.Write(delimiter);
                node.ToEPL(writer, Precedence);
                delimiter = "|";
            }
        }

        public override RowRecogExprNode CheckedCopySelf(ExpressionCopier expressionCopier)
        {
            return new RowRecogExprNodeAlteration();
        }
    }
} // end of namespace