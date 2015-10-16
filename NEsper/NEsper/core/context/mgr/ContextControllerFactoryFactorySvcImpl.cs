///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.epl.spec;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.context.mgr
{
	public class ContextControllerFactoryFactorySvcImpl : ContextControllerFactoryFactorySvc
    {
	    public ContextControllerFactory Make(ContextControllerFactoryContext factoryContext, ContextDetail detail, IList<FilterSpecCompiled> optFiltersNested, ContextStateCache contextStateCache)
        {
	        ContextControllerFactory factory;
	        if (detail is ContextDetailInitiatedTerminated) {
	            factory = new ContextControllerInitTermFactory(factoryContext, (ContextDetailInitiatedTerminated) detail, contextStateCache);
	        } else if (detail is ContextDetailPartitioned) {
	            factory = new ContextControllerPartitionedFactory(factoryContext, (ContextDetailPartitioned) detail, optFiltersNested, contextStateCache);
	        } else if (detail is ContextDetailCategory) {
	            factory = new ContextControllerCategoryFactory(factoryContext, (ContextDetailCategory) detail, optFiltersNested, contextStateCache);
	        } else if (detail is ContextDetailHash) {
	            factory = new ContextControllerHashFactory(factoryContext, (ContextDetailHash) detail, optFiltersNested, contextStateCache);
	        } else {
	            throw new UnsupportedOperationException("Context detail " + detail + " is not yet supported in a nested context");
	        }

	        return factory;
	    }
	}
} // end of namespace
