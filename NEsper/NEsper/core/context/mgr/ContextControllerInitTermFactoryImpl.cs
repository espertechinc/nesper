///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.spec;

namespace com.espertech.esper.core.context.mgr
{
	public class ContextControllerInitTermFactoryImpl 
        : ContextControllerInitTermFactoryBase 
        , ContextControllerFactory
    {
	    private readonly ContextStatePathValueBinding _binding;

	    public ContextControllerInitTermFactoryImpl(ContextControllerFactoryContext factoryContext, ContextDetailInitiatedTerminated detail)
            : base(factoryContext, detail)
        {
	        _binding = factoryContext.StateCache.GetBinding(detail);
	    }

	    public ContextStatePathValueBinding Binding
	    {
	        get { return _binding; }
	    }

	    public override ContextController CreateNoCallback(int pathId, ContextControllerLifecycleCallback callback)
        {
	        return new ContextControllerInitTerm(pathId, callback, this);
	    }

	    public override bool IsSingleInstanceContext
	    {
            get { return !ContextDetailInitiatedTerminated.IsOverlapping; }
	    }
    }
} // end of namespace
