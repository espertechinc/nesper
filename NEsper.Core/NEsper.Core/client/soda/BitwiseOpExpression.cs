///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.type;

namespace com.espertech.esper.client.soda
{
	/// <summary>
	/// Bitwise (binary) operator for binary AND, binary OR and binary XOR.
	/// </summary>
    [Serializable]
    public class BitwiseOpExpression : ExpressionBase
	{
	    private BitWiseOpEnum _binaryOp;

        /// <summary>
        /// Initializes a new instance of the <see cref="BitwiseOpExpression"/> class.
        /// </summary>
	    public BitwiseOpExpression()
	    {
	    }

	    /// <summary>
	    /// Ctor - for use to create an expression tree, without child expression.
	    /// <para/>
	    /// Use add methods to add child expressions to acts upon.
	    /// </summary>
	    /// <param name="binaryOp">the binary operator</param>
	    public BitwiseOpExpression(BitWiseOpEnum binaryOp)
	    {
	        _binaryOp = binaryOp;
	    }

	    /// <summary>Add a property to the expression.</summary>
	    /// <param name="property">to add</param>
	    /// <returns>expression</returns>
	    public BitwiseOpExpression Add(String property)
	    {
	        Children.Add(new PropertyValueExpression(property));
	        return this;
	    }

	    /// <summary>Add a constant to the expression.</summary>
	    /// <param name="object">constant to add</param>
	    /// <returns>expression</returns>
	    public BitwiseOpExpression Add(Object @object)
	    {
	        Children.Add(new ConstantExpression(@object));
	        return this;
	    }

	    /// <summary>Add an expression to the expression.</summary>
	    /// <param name="expression">to add</param>
	    /// <returns>expression</returns>
	    public BitwiseOpExpression Add(Expression expression)
	    {
	        Children.Add(expression);
	        return this;
	    }

	    public override ExpressionPrecedenceEnum Precedence
	    {
	        get { return ExpressionPrecedenceEnum.BITWISE; }
	    }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
	    {
	        bool isFirst = true;
	        foreach (Expression child in Children)
	        {
	            if (!isFirst)
	            {
	                writer.Write(_binaryOp.GetExpressionText());
	            }
	            child.ToEPL(writer, Precedence);
	            isFirst = false;
	        }
	    }

	    /// <summary>Gets or sets the binary operator.</summary>
	    /// <returns>operator</returns>
	    public BitWiseOpEnum BinaryOp
	    {
	    	get { return _binaryOp; }
	    	set { _binaryOp = value ; }
	    }
	}
} // End of namespace
