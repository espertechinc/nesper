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
	/// Concatenation expression that concatenates the result of child expressions to the expression.
	/// </summary>
    [Serializable]
    public class ConcatExpression : ExpressionBase
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcatExpression"/> class.
        /// </summary>
	    public ConcatExpression()
	    {
	    }

	    /// <summary>Add a constant to include in the computation.</summary>
	    /// <param name="object">constant to add</param>
	    /// <returns>expression</returns>
	    public ConcatExpression Add(Object @object)
	    {
	        Children.Add(new ConstantExpression(@object));
	        return this;
	    }

	    /// <summary>Add an expression to include in the computation.</summary>
	    /// <param name="expression">to add</param>
	    /// <returns>expression</returns>
	    public ConcatExpression Add(Expression expression)
	    {
	        Children.Add(expression);
	        return this;
	    }

	    /// <summary>Add a property to include in the computation.</summary>
	    /// <param name="propertyName">is the name of the property</param>
	    /// <returns>expression</returns>
	    public ConcatExpression Add(String propertyName)
	    {
	        Children.Add(new PropertyValueExpression(propertyName));
	        return this;
	    }

	    public override ExpressionPrecedenceEnum Precedence
	    {
	        get { return ExpressionPrecedenceEnum.CONCAT; }
	    }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
	    {
	        String delimiter = "";
	        foreach (Expression child in Children)
	        {
	            writer.Write(delimiter);
	            child.ToEPL(writer, Precedence);
	            delimiter = "||";
	        }
	    }
	}
} // End of namespace
