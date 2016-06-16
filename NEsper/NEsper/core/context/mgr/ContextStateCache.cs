///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.core.context.mgr
{
    public interface ContextStateCache
    {
        ContextStatePathValueBinding GetBinding(Object bindingInfo);
        void AddContextPath(String contextName, int level, int parentPath, int subPath, int? optionalContextPartitionId, Object additionalInfo, ContextStatePathValueBinding binding);
        void UpdateContextPath(String contextName, ContextStatePathKey key, ContextStatePathValue value);
        void RemoveContextParentPath(String contextName, int level, int parentPath);
        void RemoveContextPath(String contextName, int level, int parentPath, int subPath);
        void RemoveContext(String contextName);
        OrderedDictionary<ContextStatePathKey, ContextStatePathValue> GetContextPaths(String contextName);
    }
}
