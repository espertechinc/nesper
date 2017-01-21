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
	/// Case-expression that acts as a switch testing a value against other values.
	/// <para>
	/// The first child expression provides the value to switch on.
	/// The following pairs of child expressions provide the "when expression then expression" results.
	/// The last child expression provides the "else" result.
	/// </para>
	/// </summary>
    [Serializable]
    public class CaseSwitchExpression : ExpressionBase
	{
	    /// <summary>
	    /// Ctor - for use to create an expression tree, without inner expression
	    /// </summary>
	    public CaseSwitchExpression()
	    {
	    }

	    /// <summary>Ctor.</summary>
	    /// <param name="switchValue">is the expression providing the value to switch on</param>
	    public CaseSwitchExpression(Expression switchValue)
	    {
	        // switch value expression is first
	        AddChild(switchValue);
	    }

	    /// <summary>
	    /// Adds a pair of expressions representing a "when" and a "then" in the switch.
	    /// </summary>
	    /// <param name="when">expression to match on</param>
	    /// <param name="then">
	    /// expression to return a conditional result when the when-expression matches
	    /// </param>
	    /// <returns>expression</returns>
	    public CaseSwitchExpression Add(Expression when, Expression then)
	    {
	        int size = Children.Count;
	        if (size % 2 != 0)
	        {
	            AddChild(when);
	            AddChild(then);
	        }
	        else
	        {
	            // add next to last as the last node is the else clause
	            Children.Insert(Children.Count - 1, when);
	            Children.Insert(Children.Count - 1, then);
	        }
	        return this;
	    }

	    /// <summary>
	    /// Sets the else-part of the case-switch. This result of this expression is returned
	    /// when no when-expression matched.
	    /// </summary>
	    /// <param name="elseExpr">is the expression returning the no-match value</param>
	    /// <returns>expression</returns>
	    public CaseSwitchExpression SetElse(Expression elseExpr)
	    {
	        int size = Children.Count;
	        // remove last node representing the else
	        if (size % 2 == 0)
	        {
	            Children.RemoveAt(size - 1);
	        }
	        AddChild(elseExpr);
	        return this;
	    }

	    public override ExpressionPrecedenceEnum Precedence
	    {
	        get { return ExpressionPrecedenceEnum.CASE; }
	    }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
	    {
	        IList<Expression> children = Children;

	        writer.Write("case ");
            children[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
	        int index = 1;
            while (index < children.Count - 1)
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
