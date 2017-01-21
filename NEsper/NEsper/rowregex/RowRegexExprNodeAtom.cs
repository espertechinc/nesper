///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat.logging;

namespace com.espertech.esper.rowregex
{
    /// <summary>
    /// Atom in a regex expression tree.
    /// </summary>
    [Serializable]
    public class RowRegexExprNodeAtom : RowRegexExprNode
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="tag">variable name</param>
        /// <param name="type">multiplicity and greedy indicator</param>
        /// <param name="optionalRepeat">optional repeating information</param>
        public RowRegexExprNodeAtom(String tag, RegexNFATypeEnum type, RowRegexExprRepeatDesc optionalRepeat)
        {
            Tag = tag;
            NFAType = type;
            OptionalRepeat = optionalRepeat;
        }

        /// <summary>
        /// Returns the variable name.
        /// </summary>
        /// <returns>
        /// variable
        /// </returns>
        public string Tag { get; private set; }

        /// <summary>
        /// Returns multiplicity and greedy indicator.
        /// </summary>
        /// <returns>
        /// type
        /// </returns>
        public RegexNFATypeEnum NFAType { get; private set; }

        /// <summary>
        /// Gets the optional repeat.
        /// </summary>
        /// <value>
        /// The optional repeat.
        /// </value>
        public RowRegexExprRepeatDesc OptionalRepeat { get; private set; }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(Tag);
            writer.Write(NFAType.OptionalPostfix());
            if (OptionalRepeat != null)
            {
                OptionalRepeat.ToExpressionString(writer);
            }
        }

        public override RowRegexExprNodePrecedenceEnum Precedence
        {
            get { return RowRegexExprNodePrecedenceEnum.UNARY; }
        }
    }
}
