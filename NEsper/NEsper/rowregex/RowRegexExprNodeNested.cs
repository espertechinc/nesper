///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

namespace com.espertech.esper.rowregex
{
    /// <summary>Nested () regular expression in a regex expression tree.</summary>
    public class RowRegexExprNodeNested : RowRegexExprNode
    {
        private readonly RegexNFATypeEnum _type;
        private readonly RowRegexExprRepeatDesc _optionalRepeat;
    
        public RowRegexExprNodeNested(RegexNFATypeEnum type, RowRegexExprRepeatDesc optionalRepeat)
        {
            _type = type;
            _optionalRepeat = optionalRepeat;
        }

        /// <summary>
        /// Returns multiplicity and greedy.
        /// </summary>
        /// <value>type</value>
        public RegexNFATypeEnum ReturnType
        {
            get { return _type; }
        }

        public RowRegexExprRepeatDesc OptionalRepeat
        {
            get { return _optionalRepeat; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ChildNodes[0].ToEPL(writer, Precedence);
            writer.Write(_type.OptionalPostfix);
        }

        public override RowRegexExprNodePrecedenceEnum Precedence
        {
            get { return RowRegexExprNodePrecedenceEnum.GROUPING; }
        }
    }
} // end of namespace
