///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
	/// <summary>
	/// Instance-of expression checks if an expression returns a certain type.
	/// </summary>
	public class InstanceOfExpression : ExpressionBase {
	    private string[] typeNames;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    public InstanceOfExpression() {
	    }

	    /// <summary>
	    /// Ctor - for use to create an expression tree, without child expression.
	    /// </summary>
	    /// <param name="typeNames">is the fully-qualified class names or Java primitive type names or "string"</param>
	    public InstanceOfExpression(string[] typeNames) {
	        this.typeNames = typeNames;
	    }

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="expressionToCheck">provides values to check the type of</param>
	    /// <param name="typeName">is one fully-qualified class names or Java primitive type names or "string"</param>
	    /// <param name="moreTypes">is additional optional fully-qualified class names or Java primitive type names or "string"</param>
	    public InstanceOfExpression(Expression expressionToCheck, string typeName, params string[] moreTypes) {
	        this.Children.Add(expressionToCheck);
	        if (moreTypes == null) {
	            typeNames = new string[]{typeName};
	        } else {
	            typeNames = new string[moreTypes.Length + 1];
	            typeNames[0] = typeName;
	            Array.Copy(moreTypes, 0, this.typeNames, 1, moreTypes.Length);
	        }
	    }

	    public override ExpressionPrecedenceEnum Precedence
	    {
	        get => ExpressionPrecedenceEnum.UNARY;
	    }

	    public override void ToPrecedenceFreeEPL(TextWriter writer) {
	        writer.Write("instanceof(");
	        this.Children[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
	        writer.Write(",");

	        string delimiter = "";
	        foreach (string typeName in typeNames) {
	            writer.Write(delimiter);
	            writer.Write(typeName);
	            delimiter = ",";
	        }
	        writer.Write(")");
	    }

	    /// <summary>
	    /// Returns the types to compare to.
	    /// </summary>
	    /// <returns>list of types to compare to</returns>
	    public string[] TypeNames
	    {
	        get => typeNames;
	    }

	    /// <summary>
	    /// Sets the types to compare to.
	    /// </summary>
	    /// <param name="typeNames">list of types to compare to</param>
	    public void SetTypeNames(string[] typeNames) {
	        this.typeNames = typeNames;
	    }
	}
} // end of namespace