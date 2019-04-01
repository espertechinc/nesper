///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

namespace com.espertech.esper.common.@internal.epl.rowrecog.expr
{
    /// <summary>
    ///     Concatenation of atoms in a regular expression tree.
    /// </summary>
    public class RowRecogExprNodeConcatenation : RowRecogExprNode
    {
        public override RowRecogExprNodePrecedenceEnum Precedence => RowRecogExprNodePrecedenceEnum.CONCATENATION;

        public override void ToPrecedenceFreeEPL(StringWriter writer)
        {
            var delimiter = "";
            foreach (var node in ChildNodes) {
                writer.Write(delimiter);
                node.ToEPL(writer, Precedence);
                delimiter = " ";
            }
        }
    }
} // end of namespace