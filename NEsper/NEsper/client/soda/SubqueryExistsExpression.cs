///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
	/// Exists-expression for a set of values returned by a lookup.
	/// </summary>
    [Serializable]
    public class SubqueryExistsExpression : ExpressionBase
	{
	    /// <summary>
        /// Initializes a new instance of the <see cref="SubqueryExistsExpression"/> class.
        /// </summary>
	    public SubqueryExistsExpression()
	    {
	    }

	    /// <summary>
	    /// Ctor - for use to create an expression tree, without child expression.
	    /// </summary>
	    /// <param name="model">is the lookup statement object model</param>
	    public SubqueryExistsExpression(EPStatementObjectModel model)
	    {
	        Model = model;
	    }

	    public override ExpressionPrecedenceEnum Precedence
	    {
	        get { return ExpressionPrecedenceEnum.UNARY; }
	    }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
	    {
	        writer.Write("exists (");
	        writer.Write(Model.ToEPL());
	        writer.Write(')');
	    }

	    /// <summary>Gets or sets the lookup statement object model.</summary>
	    /// <returns>lookup model</returns>
	    public EPStatementObjectModel Model { get; set; }
	}
} // End of namespace
