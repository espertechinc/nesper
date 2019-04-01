///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.prior
{
	/// <summary>
	/// Represents the 'prior' prior event function in an expression node tree.
	/// </summary>
	public class ExprPriorEvalStrategyRelativeAccess : PriorEvalStrategy {
	    [NonSerialized] private readonly RelativeAccessByEventNIndex relativeAccess;

	    public ExprPriorEvalStrategyRelativeAccess(RelativeAccessByEventNIndex relativeAccess) {
	        this.relativeAccess = relativeAccess;
	    }

	    public EventBean GetSubstituteEvent(EventBean originalEvent, bool isNewData, int constantIndexNumber, int relativeIndex, ExprEvaluatorContext exprEvaluatorContext, int streamNum) {
	        return relativeAccess.GetRelativeToEvent(originalEvent, relativeIndex);
	    }
	}
} // end of namespace