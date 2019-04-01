///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.prev;
using com.espertech.esper.common.@internal.epl.expression.prior;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.access
{
	/// <summary>
	/// Coordinates between view factories and requested resource (by expressions) the
	/// availability of view resources to expressions.
	/// </summary>
	public class ViewResourceDelegateExpr {
	    private readonly IList<ExprPriorNode> priorRequests;
	    private readonly IList<ExprPreviousNode> previousRequests;

	    public ViewResourceDelegateExpr() {
	        this.priorRequests = new List<>();
	        this.previousRequests = new List<>();
	    }

	    public IList<ExprPriorNode> GetPriorRequests() {
	        return priorRequests;
	    }

	    public void AddPriorNodeRequest(ExprPriorNode priorNode) {
	        priorRequests.Add(priorNode);
	    }

	    public void AddPreviousRequest(ExprPreviousNode previousNode) {
	        previousRequests.Add(previousNode);
	    }

	    public IList<ExprPreviousNode> GetPreviousRequests() {
	        return previousRequests;
	    }
	}
} // end of namespace