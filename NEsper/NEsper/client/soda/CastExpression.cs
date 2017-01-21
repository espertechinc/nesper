///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Linq;

namespace com.espertech.esper.client.soda
{
	/// <summary>
	/// Cast expression casts the return value of an expression to a specified type.
	/// </summary>
    [Serializable]
    public class CastExpression : ExpressionBase
	{
	    private String _typeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="CastExpression"/> class.
        /// </summary>
	    public CastExpression()
	    {
	    }

	    /// <summary>
	    /// Ctor - for use to create an expression tree, without child expression.
	    /// </summary>
	    /// <param name="typeName">
	    /// is the type to cast to: a fully-qualified class name or primitive type name or "string"
	    /// </param>
	    public CastExpression(String typeName)
	    {
	        _typeName = typeName;
	    }

	    /// <summary>Ctor.</summary>
	    /// <param name="expressionToCheck">provides values to cast</param>
	    /// <param name="typeName">
	    /// is the type to cast to: a fully-qualified class names or primitive type names or "string"
	    /// </param>
	    public CastExpression(Expression expressionToCheck, String typeName)
	    {
	        Children.Add(expressionToCheck);
	        _typeName = typeName;
	    }

	    public override ExpressionPrecedenceEnum Precedence
	    {
	        get { return ExpressionPrecedenceEnum.UNARY; }
	    }

	    /// <summary>Renders the clause in textual representation.</summary>
	    /// <param name="writer">to output to</param>
        public override void ToPrecedenceFreeEPL(TextWriter writer)
	    {
	        writer.Write("cast(");
            Children[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
	        writer.Write(",");
	        writer.Write(_typeName);
            
            for (int ii = 1; ii < Children.Count ; ii++)
            {
                writer.Write(",");
                Children[ii].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
            
            writer.Write(")");
	    }

	    /// <summary>Gets or sets the name of the type to cast to.</summary>
	    /// <returns>type name</returns>
	    public String TypeName
	    {
	    	get { return _typeName; }
	    	set { _typeName = value; }
	    }
	}
} // End of namespace
