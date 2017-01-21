///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.compat;

namespace com.espertech.esper.client.soda
{
	/// <summary>
	/// Case expression that act as a when-then-else.
	/// </summary>
    
    [Serializable]
    public class CaseWhenThenExpression : ExpressionBase
	{
	    /// <summary>
	    /// Ctor - for use to create an expression tree, without child expression.
	    /// <para>
	    /// Use add methods to add child expressions to acts upon.
	    /// </para>
	    /// </summary>
	    public CaseWhenThenExpression()
	    {
	    }

	    /// <summary>Adds a when-then pair of expressions.</summary>
	    /// <param name="when">providings conditions to evaluate</param>
	    /// <param name="then">provides the result when a condition evaluates to true</param>
	    /// <returns>expression</returns>
	    public CaseWhenThenExpression Add(Expression when, Expression then)
	    {
	        int size = this.Children.Count;
	        if (size % 2 == 0)
	        {
	            this.AddChild(when);
	            this.AddChild(then);
	        }
	        else
	        {
	            // add next to last as the last node is the else clause
	            this.Children.Insert(this.Children.Count - 1, when);
	            this.Children.Insert(this.Children.Count - 1, then);
	        }
	        return this;
	    }

	    /// <summary>
	    /// Sets the expression to provide a value when no when-condition matches.
	    /// </summary>
	    /// <param name="elseExpr">expression providing default result</param>
	    /// <returns>expression</returns>
	    public CaseWhenThenExpression SetElse(Expression elseExpr)
	    {
	        int size = this.Children.Count;
	        // remove last node representing the else
	        if (size % 2 != 0)
	        {
	            this.Children.RemoveAt(size - 1);
	        }
	        this.AddChild(elseExpr);
	        return this;
	    }


	    public override ExpressionPrecedenceEnum Precedence
	    {
	        get { return ExpressionPrecedenceEnum.CASE; }
	    }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
	    {
            IList<Expression> children = Children;

	        writer.Write("case");
	        int index = 0;
	        while(index < children.Count - 1)
	        {
	            writer.Write(" when ");
                children[index].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
	            index++;
	            if (index == children.Count)
	            {
	                throw new IllegalStateException("Invalid case-when expression, count of when-to-then nodes not matching");
	            }
	            writer.Write(" then ");
                children[index].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
	            index++;
	        }

	        if (index < children.Count)
	        {
	            writer.Write(" else ");
                children[index].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
	        }
	        writer.Write(" end");
	    }
	}
} // End of namespace
