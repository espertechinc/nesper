///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.common.@internal.epl.rowrecog.expr
{
    /// <summary>
    ///     Or-condition in a regex expression tree.
    /// </summary>
    [Serializable]
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
    }
} // end of namespace