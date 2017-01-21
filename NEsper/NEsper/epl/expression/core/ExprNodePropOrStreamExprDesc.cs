///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.expression.core
{
	public class ExprNodePropOrStreamExprDesc : ExprNodePropOrStreamDesc
    {
	    public ExprNodePropOrStreamExprDesc(int streamNum, ExprNode originator)
        {
	        StreamNum = streamNum;
	        Originator = originator;
	    }

	    public int StreamNum { get; private set; }

	    public ExprNode Originator { get; private set; }

	    public string Textual
	    {
	        get
	        {
	            return "expression '" + ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(Originator) + "' against stream " + StreamNum;
	        }
	    }
    }
} // end of namespace
