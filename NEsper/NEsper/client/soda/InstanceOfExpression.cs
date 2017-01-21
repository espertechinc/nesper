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
using System.Linq;

namespace com.espertech.esper.client.soda
{
	/// <summary>
	/// Instance-of expression checks if an expression returns a certain type.
	/// </summary>
    [Serializable]
    public class InstanceOfExpression : ExpressionBase
	{
	    private String[] _typeNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceOfExpression"/> class.
        /// </summary>
	    public InstanceOfExpression()
	    {
	    }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceOfExpression"/> class.
        /// </summary>
        /// <param name="typeNames">The type names.</param>
	    public InstanceOfExpression(string[] typeNames)
	    {
	        _typeNames = typeNames;
	    }

	    /// <summary>
	    /// Ctor - for use to create an expression tree, without child expression.
	    /// </summary>
	    /// <param name="typeNames">
	    /// is the fully-qualified class names or primitive type names or "string"
	    /// </param>
	    public InstanceOfExpression(IList<String> typeNames)
	    {
	        _typeNames = typeNames.ToArray();
	    }

	    /// <summary>Ctor.</summary>
	    /// <param name="expressionToCheck">provides values to check the type of</param>
	    /// <param name="typeName">
	    /// is one fully-qualified class names or primitive type names or "string"
	    /// </param>
	    /// <param name="moreTypes">
	    /// is additional optional fully-qualified class names or primitive type names or "string"
	    /// </param>
	    public InstanceOfExpression(Expression expressionToCheck, String typeName, params String[] moreTypes)
	    {
	        Children.Add(expressionToCheck);
	        if (moreTypes == null)
	        {
	            _typeNames = new String[] {typeName};
	        }
	        else
	        {
                String[] tempList = new String[moreTypes.Length + 1];
	            tempList[0] = typeName;
                Array.Copy(moreTypes, 0, tempList, 1, moreTypes.Length);
	            _typeNames = tempList;
	        }
	    }
        
	    public override ExpressionPrecedenceEnum Precedence
	    {
	        get { return ExpressionPrecedenceEnum.UNARY; }
	    }

	    /// <summary>Renders the clause in textual representation.</summary>
	    /// <param name="writer">to output to</param>
        public override void ToPrecedenceFreeEPL(TextWriter writer)
	    {
	        writer.Write("instanceof(");
            Children[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
	        writer.Write(",");

	        String delimiter = "";
	        foreach (String typeName in _typeNames)
	        {
	            writer.Write(delimiter);
	            writer.Write(typeName);
	            delimiter = ",";
	        }
	        writer.Write(")");
	    }

	    /// <summary>Gets or sets the types to compare to.</summary>
	    /// <returns>list of types to compare to</returns>
	    public string[] TypeNames
	    {
	    	get { return _typeNames; }
	    	set { _typeNames = value ; }
	    }
	}
} // End of namespace
