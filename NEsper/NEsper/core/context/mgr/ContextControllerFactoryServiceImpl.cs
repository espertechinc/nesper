///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.core.context.mgr
{
	public class ContextControllerFactoryServiceImpl : ContextControllerFactoryService
    {
	    private readonly static ContextStateCache CACHE_NO_SAVE = new ContextStateCacheNoSave();

	    public readonly static ContextControllerFactoryServiceImpl DEFAULT_FACTORY = new ContextControllerFactoryServiceImpl(CACHE_NO_SAVE);

	    private readonly ContextStateCache _cache;

	    public ContextControllerFactoryServiceImpl(ContextStateCache cache) {
	        _cache = cache;
	    }

	    public ContextControllerFactory[] GetFactory(ContextControllerFactoryServiceContext serviceContext) {
	        return ContextControllerFactoryHelper.GetFactory(serviceContext, _cache);
	    }

	    public ContextPartitionIdManager AllocatePartitionIdMgr(string contextName, int contextStmtId) {
	        return new ContextPartitionIdManagerImpl();
	    }
	}
} // end of namespace
