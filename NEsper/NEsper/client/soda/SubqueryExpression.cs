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
	/// Subquery-expression returns values returned by a lookup modelled by a further <see cref="EPStatementObjectModel"/>.
	/// </summary>
    [Serializable]
    public class SubqueryExpression : ExpressionBase
	{
	    /// <summary>
        /// Initializes a new instance of the <see cref="SubqueryExpression"/> class.
        /// </summary>
	    public SubqueryExpression()
	    {
	    }

	    /// <summary>
	    /// Ctor - for use to create an expression tree, without child expression.
	    /// </summary>
	    /// <param name="model">is the lookup statement object model</param>
	    public SubqueryExpression(EPStatementObjectModel model)
	    {
	        Model = model;
	    }

	    public override ExpressionPrecedenceEnum Precedence
	    {
	        get { return ExpressionPrecedenceEnum.UNARY; }
	    }

	    public override void ToPrecedenceFreeEPL(TextWriter writer)
	    {
	        writer.Write('(');
	        writer.Write(Model.ToEPL());
	        writer.Write(')');
	    }

	    /// <summary>Gets or sets the lookup statement object model.</summary>
	    /// <returns>lookup model</returns>
	    public EPStatementObjectModel Model { get; set; }
	}
} // End of namespace
