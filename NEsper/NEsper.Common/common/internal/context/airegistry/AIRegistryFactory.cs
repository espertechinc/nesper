///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.airegistry
{
	public interface AIRegistryFactory {
	    AIRegistryPriorEvalStrategy MakePrior();

	    AIRegistryPreviousGetterStrategy MakePrevious();

	    AIRegistrySubselectLookup MakeSubqueryLookup(LookupStrategyDesc lookupStrategyDesc);

	    AIRegistryAggregation MakeAggregation();

	    AIRegistryTableAccess MakeTableAccess();

	    AIRegistryRowRecogPreviousStrategy MakeRowRecogPreviousStrategy();
	}
} // end of namespace