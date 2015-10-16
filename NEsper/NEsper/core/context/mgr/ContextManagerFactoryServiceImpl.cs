///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.spec;

namespace com.espertech.esper.core.context.mgr
{
	public class ContextManagerFactoryServiceImpl : ContextManagerFactoryService
    {
	    public ContextManager Make(ContextDetail contextDetail, ContextControllerFactoryServiceContext factoryServiceContext)
        {
	        if (contextDetail is ContextDetailNested)
            {
	            return new ContextManagerNested(factoryServiceContext);
	        }
	        return new ContextManagerImpl(factoryServiceContext);
	    }
	}
} // end of namespace
