///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.parse
{
	public class ASTSubstitutionHelper
	{
	    public static void ValidateNewSubstitution(IList<ExprSubstitutionNode> substitutionParamNodes, ExprSubstitutionNode substitutionNode)
        {
	        if (substitutionParamNodes.IsEmpty()) {
	            return;
	        }
	        ExprSubstitutionNode first = substitutionParamNodes[0];
	        if (substitutionNode.Index != null && first.Index == null) {
	            throw GetException();
	        }
	        if (substitutionNode.Name != null && first.Name == null) {
	            throw GetException();
	        }
	    }

	    private static ASTWalkException GetException()
        {
	        return ASTWalkException.From("Inconsistent use of substitution parameters, expecting all substitutions to either all provide a name or provide no name");
	    }
	}

} // end of namespace
