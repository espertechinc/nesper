///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    public class ContextControllerHashFactoryImpl 
        : ContextControllerHashFactoryBase
        , ContextControllerFactory
    {
        private readonly ContextStatePathValueBinding _binding;

        public ContextControllerHashFactoryImpl(
            ContextControllerFactoryContext factoryContext,
            ContextDetailHash hashedSpec,
            IList<FilterSpecCompiled> filtersSpecsNestedContexts)
            : base(factoryContext, hashedSpec, filtersSpecsNestedContexts)
        {
            _binding = factoryContext.StateCache.GetBinding(typeof(int));
        }

        public override ContextController CreateNoCallback(int pathId, ContextControllerLifecycleCallback callback)
        {
            return new ContextControllerHash(pathId, callback, this);
        }

        public ContextStatePathValueBinding Binding
        {
            get { return _binding; }
        }
    }
} // end of namespace