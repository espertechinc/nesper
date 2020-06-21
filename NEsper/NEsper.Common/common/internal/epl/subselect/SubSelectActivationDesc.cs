///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
/*
 ***************************************************************************************
 *  Copyright (C) 2006 EsperTech, Inc. All rights reserved.                            *
 *  http://www.espertech.com/esper                                                     *
 *  http://www.espertech.com                                                           *
 *  ---------------------------------------------------------------------------------- *
 *  The software in this package is published under the terms of the GPL license       *
 *  a copy of which has been included with this distribution in the license.txt file.  *
 ***************************************************************************************
 */

namespace com.espertech.esper.common.@internal.epl.subselect
{
	public class SubSelectActivationDesc {

	    private readonly IDictionary<ExprSubselectNode, SubSelectActivationPlan> subselects;
	    private readonly IList<StmtClassForgeableFactory> additionalForgeables;

	    public SubSelectActivationDesc(IDictionary<ExprSubselectNode, SubSelectActivationPlan> subselects, IList<StmtClassForgeableFactory> additionalForgeables) {
	        this.subselects = subselects;
	        this.additionalForgeables = additionalForgeables;
	    }

	    public IDictionary<ExprSubselectNode, SubSelectActivationPlan> GetSubselects() {
	        return subselects;
	    }

	    public IList<StmtClassForgeableFactory> GetAdditionalForgeables() {
	        return additionalForgeables;
	    }
	}
} // end of namespace
