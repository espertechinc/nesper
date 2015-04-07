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
	/// Current execution context supplies the current expression execution context.
	/// </summary>
	[Serializable]
    public class CurrentEvaluationContextExpression : ExpressionBase
	{
	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    public CurrentEvaluationContextExpression()
	    {
	    }

	    public override ExpressionPrecedenceEnum Precedence
	    {
	        get { return ExpressionPrecedenceEnum.UNARY; }
	    }

	    public override void ToPrecedenceFreeEPL(TextWriter writer)
	    {
	        writer.Write("current_evaluation_context()");
	    }
	}
} // end of namespace
