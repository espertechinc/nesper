///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.rowregex
{
    /// <summary>
    /// Or-condition in a regex expression tree.
    /// </summary>
    [Serializable]
    public class RowRegexExprNodeAlteration : RowRegexExprNode
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public RowRegexExprNodeAlteration()
        {        
        }
    
        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            String delimiter = "";
            foreach (RowRegexExprNode node in this.ChildNodes)
            {
                writer.Write(delimiter);
                node.ToEPL(writer, Precedence);
                delimiter = "|";
            }
        }

        public override RowRegexExprNodePrecedenceEnum Precedence
        {
            get { return RowRegexExprNodePrecedenceEnum.ALTERNATION; }
        }
    }
}
