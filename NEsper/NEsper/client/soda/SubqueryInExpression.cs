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
    /// In-expression for a set of values returned by a lookup.
	/// </summary>
    [Serializable]
    public class SubqueryInExpression : ExpressionBase
	{
	    private bool isNotIn;
	    private EPStatementObjectModel model;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubqueryInExpression"/> class.
        /// </summary>
	    public SubqueryInExpression()
	    {
	    }

	    /// <summary>
	    /// Ctor - for use to create an expression tree, without child expression.
	    /// </summary>
	    /// <param name="model">is the lookup statement object model</param>
	    /// <param name="isNotIn">is true for not-in</param>
	    public SubqueryInExpression(EPStatementObjectModel model, bool isNotIn)
	    {
	        this.model = model;
	        this.isNotIn = isNotIn;
	    }

	    /// <summary>
	    /// Ctor - for use to create an expression tree, without child expression.
	    /// </summary>
	    /// <param name="expression">is the expression providing the value to match</param>
	    /// <param name="model">is the lookup statement object model</param>
	    /// <param name="isNotIn">is true for not-in</param>
	    public SubqueryInExpression(Expression expression, EPStatementObjectModel model, bool isNotIn)
	    {
	        this.Children.Add(expression);
	        this.model = model;
	        this.isNotIn = isNotIn;
	    }

	    /// <summary>Gets or sets the true for not-in, or false for in-lookup.</summary>
	    /// <returns>true for not-in</returns>
	    public bool IsNotIn
	    {
	    	get { return isNotIn; }
	    	set { isNotIn = value; }
	    }

	    public override ExpressionPrecedenceEnum Precedence
	    {
	        get { return ExpressionPrecedenceEnum.UNARY; }
	    }

	    public override void ToPrecedenceFreeEPL(TextWriter writer)
	    {
	        Children[0].ToEPL(writer, Precedence);
	        if (IsNotIn) {
	            writer.Write(" not in (");
	        }
	        else {
	            writer.Write(" in (");
	        }
	        writer.Write(model.ToEPL());
	        writer.Write(')');
	    }

	    /// <summary>Gets or sets the lookup statement object model.</summary>
	    /// <returns>lookup model</returns>
	    public EPStatementObjectModel Model
	    {
	    	get { return model; }
	    	set { model = value; }
	    }
	}
} // End of namespace
