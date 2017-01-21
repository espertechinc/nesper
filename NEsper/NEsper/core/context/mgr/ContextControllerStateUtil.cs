///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerStateUtil {
    
        public static ContextControllerState GetRecoveryStates(ContextStateCache cache, String contextName)
        {
            OrderedDictionary<ContextStatePathKey, ContextStatePathValue> state = cache.GetContextPaths(contextName);
            if (state == null || state.IsEmpty()) {
                return null;
            }
            return new ContextControllerState(state, false, null);
        }
    
        public static IDictionary<ContextStatePathKey, ContextStatePathValue> GetChildContexts(ContextControllerFactoryContext factoryContext, int pathId, OrderedDictionary<ContextStatePathKey, ContextStatePathValue> states) 
        {
            ContextStatePathKey start = new ContextStatePathKey(factoryContext.NestingLevel, pathId, int.MinValue);
            ContextStatePathKey end = new ContextStatePathKey(factoryContext.NestingLevel, pathId, int.MaxValue);
            return states.Between(start, true, end, true);
        }
    }
    
}
