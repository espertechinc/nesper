///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.@internal.epl.rowrecog.core;

namespace com.espertech.esper.common.@internal.epl.rowrecog.expr
{
    /// <summary>
    ///     Nested () regular expression in a regex expression tree.
    /// </summary>
    [Serializable]
    public class RowRecogExprNodeNested : RowRecogExprNode
    {
        public RowRecogExprNodeNested(
            RowRecogNFATypeEnum type,
            RowRecogExprRepeatDesc optionalRepeat)
        {
            Type = type;
            OptionalRepeat = optionalRepeat;
        }

        /// <summary>
        ///     Returns multiplicity and greedy.
        /// </summary>
        /// <returns>type</returns>
        public RowRecogNFATypeEnum Type { get; }

        public RowRecogExprRepeatDesc OptionalRepeat { get; }

        public override RowRecogExprNodePrecedenceEnum Precedence => RowRecogExprNodePrecedenceEnum.GROUPING;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ChildNodes[0].ToEPL(writer, Precedence);
            writer.Write(Type.GetOptionalPostfix());
        }
    }
} // end of namespace