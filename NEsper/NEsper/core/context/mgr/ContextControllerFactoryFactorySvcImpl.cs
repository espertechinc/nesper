///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
        public ContextControllerFactory Make(ContextControllerFactoryContext factoryContext, ContextDetail detail, IList<FilterSpecCompiled> optFiltersNested)
        {
            ContextControllerFactory factory;
            if (detail is ContextDetailInitiatedTerminated) {
                factory = new ContextControllerInitTermFactoryImpl(factoryContext, (ContextDetailInitiatedTerminated) detail);
            } else if (detail is ContextDetailPartitioned) {
                factory = new ContextControllerPartitionedFactoryImpl(factoryContext, (ContextDetailPartitioned) detail, optFiltersNested);
            } else if (detail is ContextDetailCategory) {
                factory = new ContextControllerCategoryFactoryImpl(factoryContext, (ContextDetailCategory) detail, optFiltersNested);
            } else if (detail is ContextDetailHash) {
                factory = new ContextControllerHashFactoryImpl(factoryContext, (ContextDetailHash) detail, optFiltersNested);
            } else {
                throw new UnsupportedOperationException("Context detail " + detail + " is not yet supported in a nested context");
            }

            return factory;
        }
	}
} // end of namespace
