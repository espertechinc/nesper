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
    ///     Atom in a regex expression tree.
    /// </summary>
    [Serializable]
    public class RowRecogExprNodeAtom : RowRecogExprNode
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="tag">variable name</param>
        /// <param name="type">multiplicity and greedy indicator</param>
        /// <param name="optionalRepeat">optional repeating information</param>
        public RowRecogExprNodeAtom(
            string tag,
            RowRecogNFATypeEnum type,
            RowRecogExprRepeatDesc optionalRepeat)
        {
            Tag = tag;
            Type = type;
            OptionalRepeat = optionalRepeat;
        }

        /// <summary>
        ///     Returns the variable name.
        /// </summary>
        /// <returns>variable</returns>
        public string Tag { get; }

        /// <summary>
        ///     Returns multiplicity and greedy indicator.
        /// </summary>
        /// <returns>type</returns>
        public RowRecogNFATypeEnum Type { get; }

        public RowRecogExprRepeatDesc OptionalRepeat { get; }

        public override RowRecogExprNodePrecedenceEnum Precedence => RowRecogExprNodePrecedenceEnum.UNARY;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(Tag);
            writer.Write(Type.GetOptionalPostfix());
            if (OptionalRepeat != null) {
                OptionalRepeat.ToExpressionString(writer);
            }
        }
    }
} // end of namespace