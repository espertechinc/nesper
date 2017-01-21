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
	/// The "new instance" operator instantiates a host language object.
	/// </summary>
	[Serializable]
    public class NewInstanceOperatorExpression : ExpressionBase
    {
	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    public NewInstanceOperatorExpression()
        {
	    }

	    /// <summary>
	    /// Ctor.
	    /// <para /></summary>
	    /// <param name="className">the class name</param>
	    public NewInstanceOperatorExpression(string className)
        {
	        ClassName = className;
	    }

	    public string ClassName { get; set; }

	    public override ExpressionPrecedenceEnum Precedence
	    {
	        get { return ExpressionPrecedenceEnum.UNARY; }
	    }

	    public override void ToPrecedenceFreeEPL(TextWriter writer)
	    {
	        writer.Write("new ");
	        writer.Write(ClassName);
	        writer.Write("(");
	        ExpressionBase.ToPrecedenceFreeEPL(this.Children, writer);
	        writer.Write(")");
	    }
	}
} // end of namespace
