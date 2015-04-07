///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.client.soda
{
	/// <summary>
	/// Named parameter expression of the form "name:expression" or "name:(expression, expression...)"
	/// </summary>
	[Serializable]
    public class NamedParameterExpression : ExpressionBase
	{
	    /// <summary>
	    /// Ctor - for use to create an expression tree, without child expression.
	    /// <para />Use add methods to add child expressions to acts upon.
	    /// </summary>
	    public NamedParameterExpression()
	    {
	    }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedParameterExpression"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
	    public NamedParameterExpression(string name)
        {
	        Name = name;
	    }

        /// <summary>
        /// Gets or sets the parameter name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
	    public string Name { get; set; }

	    public override ExpressionPrecedenceEnum Precedence
	    {
	        get { return ExpressionPrecedenceEnum.UNARY; }
	    }

	    public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
	        writer.Write(Name);
	        writer.Write(':');
	        if (Children.Count > 1 || Children.IsEmpty()) {
	            writer.Write('(');
	        }

	        string delimiter = "";
	        foreach (Expression expr in Children)
	        {
	            writer.Write(delimiter);
	            expr.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
	            delimiter = ",";
	        }
	        if (Children.Count > 1 || Children.IsEmpty()) {
	            writer.Write(')');
	        }
	    }
	}
} // end of namespace
