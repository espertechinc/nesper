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
    /// Nested () regular expression in a regex expression tree.
    /// </summary>
    [Serializable]
    public class RowRegexExprNodeNested : RowRegexExprNode
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="type">multiplicity and greedy</param>
        public RowRegexExprNodeNested(RegexNFATypeEnum type, RowRegexExprRepeatDesc optionalRepeat)
        {
            NFAType = type;
            OptionalRepeat = optionalRepeat;
        }

        /// <summary>
        /// Returns multiplicity and greedy.
        /// </summary>
        /// <returns>
        /// type
        /// </returns>
        public RegexNFATypeEnum NFAType { get; private set; }

        /// <summary>
        /// Returns the optional repeat information.
        /// </summary>
        /// <value>
        /// The optional repeat.
        /// </value>
        public RowRegexExprRepeatDesc OptionalRepeat { get; private set; }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ChildNodes[0].ToEPL(writer, Precedence);
            writer.Write(NFAType.OptionalPostfix());
        }

        public override RowRegexExprNodePrecedenceEnum Precedence
        {
            get { return RowRegexExprNodePrecedenceEnum.GROUPING; }
        }
    }
}
