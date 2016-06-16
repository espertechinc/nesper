///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.spec;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.context.mgr
{
	public class ContextControllerCategoryFactoryImpl : ContextControllerCategoryFactoryBase
    {
	    private readonly ContextStatePathValueBinding _binding;

	    public ContextControllerCategoryFactoryImpl(ContextControllerFactoryContext factoryContext, ContextDetailCategory categorySpec, IList<FilterSpecCompiled> filtersSpecsNestedContexts)
            : base(factoryContext, categorySpec, filtersSpecsNestedContexts)
        {
	        _binding = factoryContext.StateCache.GetBinding(typeof(int));    // the integer ordinal of the category
	    }

	    public ContextStatePathValueBinding Binding
	    {
	        get { return _binding; }
	    }

	    public override ContextController CreateNoCallback(int pathId, ContextControllerLifecycleCallback callback)
        {
	        return new ContextControllerCategory(pathId, callback, this);
	    }
	}
} // end of namespace
