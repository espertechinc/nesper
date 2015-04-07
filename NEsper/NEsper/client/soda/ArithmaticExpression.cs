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
	/// Arithmatic expression for addition, subtraction, multiplication, division and modulo.
	/// </summary>
    [Serializable]
    public class ArithmaticExpression : ExpressionBase
	{
	    /// <summary>
        /// Initializes a new instance of the <see cref="ArithmaticExpression"/> class.
        /// </summary>
	    public ArithmaticExpression()
	    {
	    }

	    /// <summary>Ctor.</summary>
	    /// <param name="operator">can be any of '-', '+', '*', '/' or '%' (modulo).</param>
	    public ArithmaticExpression(String @operator)
	    {
	        Operator = @operator;
	    }

	    /// <summary>Ctor.</summary>
	    /// <param name="left">the left hand side</param>
	    /// <param name="operator">can be any of '-', '+', '*', '/' or '%' (modulo).</param>
	    /// <param name="right">the right hand side</param>
	    public ArithmaticExpression(Expression left, String @operator, Expression right)
	    {
	        Operator = @operator;
	        AddChild(left);
	        AddChild(right);
	    }

	    /// <summary>Gets the arithmatic operator.</summary>
	    /// <returns>operator</returns>
	    public string Operator { get; set; }

	    /// <summary>Add a constant to include in the computation.</summary>
	    /// <param name="obj">constant to add</param>
	    /// <returns>expression</returns>
	    public ArithmaticExpression Add(Object obj)
	    {
	        Children.Add(new ConstantExpression(obj));
	        return this;
	    }

	    /// <summary>Add an expression to include in the computation.</summary>
	    /// <param name="expression">to add</param>
	    /// <returns>expression</returns>
	    public ArithmaticExpression Add(Expression expression)
	    {
	        Children.Add(expression);
	        return this;
	    }

	    /// <summary>Add a property to include in the computation.</summary>
	    /// <param name="propertyName">is the name of the property</param>
	    /// <returns>expression</returns>
	    public ArithmaticExpression Add(String propertyName)
	    {
	        Children.Add(new PropertyValueExpression(propertyName));
	        return this;
	    }

	    public override ExpressionPrecedenceEnum Precedence
	    {
	        get
	        {
	            switch (Operator) {
	                case "*":
	                case "/":
	                case "%":
	                    return ExpressionPrecedenceEnum.MULTIPLY;
	                default:
	                    return ExpressionPrecedenceEnum.ADDITIVE;
	            }
	        }
	    }

	    public override void ToPrecedenceFreeEPL(TextWriter writer)
	    {
	        String delimiter = "";
	        foreach (Expression child in Children)
	        {
	            writer.Write(delimiter);
	            child.ToEPL(writer, Precedence);
	            delimiter = Operator;
	        }
	    }
	}
} // End of namespace
