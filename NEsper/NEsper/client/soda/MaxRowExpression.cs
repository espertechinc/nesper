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
    /// Maximum-value per-row expression (not aggregating) determines the maximum value among a set of values.
    /// </summary>
    [Serializable]
    public class MaxRowExpression : ExpressionBase
	{
	    /// <summary>
	    /// Ctor - for use to create an expression tree, without child expression.
	    /// <para/>
	    /// Use add methods to add child expressions to acts upon.
	    /// </summary>
	    public MaxRowExpression()
	    {
	    }

	    /// <summary>Ctor.</summary>
	    /// <param name="propertyOne">
	    /// the name of the property providing a value to determine the maximum of
	    /// </param>
	    /// <param name="propertyTwo">
	    /// the name of the property providing a value to determine the maximum of
	    /// </param>
	    /// <param name="moreProperties">optional additional properties to consider</param>
	    public MaxRowExpression(String propertyOne, String propertyTwo, String[] moreProperties)
	    {
	        AddChild(new PropertyValueExpression(propertyOne));
	        AddChild(new PropertyValueExpression(propertyTwo));
	        for (int i = 0; i < moreProperties.Length; i++)
	        {
	            AddChild(new PropertyValueExpression(moreProperties[i]));
	        }
	    }

	    /// <summary>Ctor.</summary>
	    /// <param name="exprOne">provides a value to determine the maximum of</param>
	    /// <param name="exprTwo">provides a value to determine the maximum of</param>
	    /// <param name="moreExpressions">optional additional values to consider</param>
	    public MaxRowExpression(Expression exprOne, Expression exprTwo, params Expression[] moreExpressions)
	    {
	        AddChild(exprOne);
	        AddChild(exprTwo);
            for (int i = 0; i < moreExpressions.Length; i++)
	        {
	            AddChild(moreExpressions[i]);
	        }
	    }

	    /// <summary>Add a constant to include in the computation.</summary>
	    /// <param name="object">constant to add</param>
	    /// <returns>expression</returns>
	    public MaxRowExpression Add(Object @object)
	    {
	        Children.Add(new ConstantExpression(@object));
	        return this;
	    }

	    /// <summary>Add an expression to include in the computation.</summary>
	    /// <param name="expression">to add</param>
	    /// <returns>expression</returns>
	    public MaxRowExpression Add(Expression expression)
	    {
	        Children.Add(expression);
	        return this;
	    }

	    /// <summary>Add a property to include in the computation.</summary>
	    /// <param name="propertyName">is the name of the property</param>
	    /// <returns>expression</returns>
	    public MaxRowExpression Add(String propertyName)
	    {
	        Children.Add(new PropertyValueExpression(propertyName));
	        return this;
	    }

        public override ExpressionPrecedenceEnum Precedence
        {
            get { return ExpressionPrecedenceEnum.UNARY; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
	    {
	        writer.Write("max(");

	        String delimiter = "";
	        foreach (Expression expr in this.Children)
	        {
	            writer.Write(delimiter);
                expr.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
	            delimiter = ",";
	        }
	        writer.Write(')');
	    }
	}
} // End of namespace
