///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

using com.espertech.esper.client.soda;
using com.espertech.esper.core.service;

namespace com.espertech.esper.epl.spec
{
	/// <summary>
	/// Substitution parameter that represents a node in an expression tree for which to supply a parameter value
	/// before statement creation time.
	/// </summary>
	public abstract class SubstitutionParameterExpressionBase : ExpressionBase
	{
	    private object _constant;
	    private bool _isSatisfied;

        protected abstract void ToPrecedenceFreeEPLUnsatisfied(TextWriter writer);

	    public override ExpressionPrecedenceEnum Precedence
	    {
	        get { return ExpressionPrecedenceEnum.UNARY; }
	    }

	    public override void ToPrecedenceFreeEPL(TextWriter writer)
	    {
	        if (!_isSatisfied)
	        {
	            ToPrecedenceFreeEPLUnsatisfied(writer);
	        }
	        else
	        {
	            EPStatementObjectModelHelper.RenderEPL(writer, _constant);
	        }
	    }

	    /// <summary>
	    /// Returns the constant value that the expression represents.
	    /// </summary>
	    /// <value>value of constant</value>
	    public object Constant
	    {
	        get { return _constant; }
	        set
	        {
	            _constant = value;
	            _isSatisfied = true;
	        }
	    }

	    /// <summary>
	    /// Returns true if the parameter is satisfied, or false if not.
	    /// </summary>
	    /// <value>true if the actual value is supplied, false if not</value>
	    public bool IsSatisfied
	    {
	        get { return _isSatisfied; }
	    }
	}
} // end of namespace
