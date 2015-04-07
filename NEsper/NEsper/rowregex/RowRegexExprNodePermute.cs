///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
	/// Permute () regular expression in a regex expression tree.
	/// </summary>
	[Serializable]
    public class RowRegexExprNodePermute : RowRegexExprNode
	{
	    public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
	        string delimiter = "";
	        writer.Write("match_recognize_permute(");
	        foreach (RowRegexExprNode node in this.ChildNodes) {
	            writer.Write(delimiter);
	            node.ToEPL(writer, Precedence);
	            delimiter = ", ";
	        }
	        writer.Write(")");
	    }

	    public override RowRegexExprNodePrecedenceEnum Precedence
	    {
	        get { return RowRegexExprNodePrecedenceEnum.UNARY; }
	    }
	}
} // end of namespace
