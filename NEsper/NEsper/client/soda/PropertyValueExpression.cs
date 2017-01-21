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
	/// Expression returning a property value.
	/// </summary>
    [Serializable]
    public class PropertyValueExpression : ExpressionBase
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueExpression"/> class.
        /// </summary>
	    public PropertyValueExpression()
	    {
	    }

	    /// <summary>Ctor.</summary>
	    /// <param name="propertyName">is the name of the property</param>
	    public PropertyValueExpression(String propertyName)
	    {
	        PropertyName = propertyName.Trim();
	    }

	    /// <summary>Gets or sets the property name.</summary>
	    /// <returns>name of the property</returns>
	    public string PropertyName { get; set; }

        /// <summary>
        /// Gets the Precedence.
        /// </summary>
        /// <value>The Precedence.</value>
	    public override ExpressionPrecedenceEnum Precedence
	    {
	        get { return ExpressionPrecedenceEnum.UNARY; }
	    }

	    /// <summary>
        /// Renders the expressions and all it's child expression, in full tree depth, as a string in
        /// language syntax.
        /// </summary>
        /// <param name="writer">is the output to use</param>
        public override void ToPrecedenceFreeEPL(TextWriter writer)
	    {
	        writer.Write(PropertyName);
	    }
	}
} // End of namespace
