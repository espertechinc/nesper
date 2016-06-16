///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.epl.spec;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.context.mgr
{
	public class ContextControllerPartitionedFactoryImpl
        : ContextControllerPartitionedFactoryBase
        , ContextControllerFactory
    {
	    private readonly ContextStatePathValueBinding _binding;

	    public ContextControllerPartitionedFactoryImpl(ContextControllerFactoryContext factoryContext, ContextDetailPartitioned segmentedSpec, IList<FilterSpecCompiled> filtersSpecsNestedContexts)
            : base(factoryContext, segmentedSpec, filtersSpecsNestedContexts)
        {
	        _binding = factoryContext.StateCache.GetBinding(typeof(ContextControllerPartitionedState));
	    }

	    public ContextStatePathValueBinding Binding
	    {
	        get { return _binding; }
	    }

	    public override ContextController CreateNoCallback(int pathId, ContextControllerLifecycleCallback callback)
        {
	        return new ContextControllerPartitioned(pathId, callback, this);
	    }
	}
} // end of namespace
