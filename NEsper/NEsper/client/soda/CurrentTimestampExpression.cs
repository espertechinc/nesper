///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.client.soda
{
	/// <summary>
	/// Current timestamp supplies the current engine time in an expression.
	/// </summary>
    [Serializable]
    public class CurrentTimestampExpression : ExpressionBase
	{
	    /// <summary>Ctor.</summary>
	    public CurrentTimestampExpression()
	    {
	    }

	    public override ExpressionPrecedenceEnum Precedence
	    {
	        get { return ExpressionPrecedenceEnum.UNARY; }
	    }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
	    {
	        writer.Write("current_timestamp()");
	    }
	}
} // End of namespace
