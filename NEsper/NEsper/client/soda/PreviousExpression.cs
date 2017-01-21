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
	/// Previous function for obtaining property values of previous events.
	/// </summary>
    [Serializable]
    public class PreviousExpression : ExpressionBase
	{
        private PreviousExpressionType _expressionType =
            PreviousExpressionType.PREV;

	    /// <summary>Ctor.</summary>
	    public PreviousExpression()
	    {
	    }

	    /// <summary>Ctor.</summary>
	    /// <param name="expression">provides the index to use</param>
	    /// <param name="propertyName">
	    /// is the name of the property to return the value for
	    /// </param>
	    public PreviousExpression(Expression expression, String propertyName)
	    {
	        AddChild(expression);
	        AddChild(new PropertyValueExpression(propertyName));
	    }

	    /// <summary>Ctor.</summary>
	    /// <param name="index">provides the index</param>
	    /// <param name="propertyName">
	    /// is the name of the property to return the value for
	    /// </param>
	    public PreviousExpression(int index, String propertyName)
	    {
	        AddChild(new ConstantExpression(index));
	        AddChild(new PropertyValueExpression(propertyName));
	    }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="expressionType">Type of the expression.</param>
        /// <param name="expression">to evaluate</param>
        public PreviousExpression(PreviousExpressionType expressionType, Expression expression)
        {
            _expressionType = expressionType;
            AddChild(expression);
        }

	    public override ExpressionPrecedenceEnum Precedence
	    {
	        get { return ExpressionPrecedenceEnum.UNARY; }
	    }

	    public PreviousExpressionType ExpressionType
	    {
            get { return _expressionType; }
            set { _expressionType = value; }
	    }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(_expressionType.ToString().ToLower());
            writer.Write("("); 
            Children[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            if (Children.Count > 1)
            {
                writer.Write(",");
                Children[1].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
            writer.Write(')');
        }
	}
} // End of namespace
