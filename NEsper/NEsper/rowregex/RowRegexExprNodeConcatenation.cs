///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.rowregex
{
    /// <summary>
    /// Concatenation of atoms in a regular expression tree.
    /// </summary>
    [Serializable]
    public class RowRegexExprNodeConcatenation : RowRegexExprNode
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        /// <summary>
        /// Ctor.
        /// </summary>
        public RowRegexExprNodeConcatenation()
        {        
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            String delimiter = "";
            foreach (RowRegexExprNode node in ChildNodes)
            {
                writer.Write(delimiter);
                node.ToEPL(writer, Precedence);
                delimiter = " ";
            }
        }

        public override RowRegexExprNodePrecedenceEnum Precedence
        {
            get { return RowRegexExprNodePrecedenceEnum.CONCATENATION; }
        }
    }
}
