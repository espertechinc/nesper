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
	/// A single entry in an order-by clause consisting of an expression and order ascending or descending flag.
	/// </summary>
	[Serializable]
	public class OrderByElement
	{
	    /// <summary>
        /// Initializes a new instance of the <see cref="OrderByElement"/> class.
        /// </summary>
	    public OrderByElement()
	    {
	    }

	    /// <summary>Ctor.</summary>
	    /// <param name="expression">is the expression to order by</param>
	    /// <param name="descending">true for descending sort, false for ascending sort</param>
	    public OrderByElement(Expression expression, bool descending)
	    {
	        Expression = expression;
	        IsDescending = descending;
	    }

	    /// <summary>Gets or sets the order-by value expression.</summary>
	    /// <returns>expression</returns>
	    public Expression Expression { get; set; }

	    /// <summary>
	    /// True for descending sorts for this column, false for ascending sort.
	    /// </summary>
	    /// <returns>true for descending sort</returns>
	    public bool IsDescending { get; set; }

	    /// <summary>Renders the clause in textual representation.</summary>
	    /// <param name="writer">to output to</param>
	    public void ToEPL(TextWriter writer)
	    {
            Expression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
	        if (IsDescending)
	        {
	            writer.Write(" desc");
	        }
	    }
	}
} // End of namespace
