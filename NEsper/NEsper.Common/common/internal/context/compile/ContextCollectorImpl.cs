///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.compile
{
    public class ContextCollectorImpl : ContextCollector
    {
        private readonly IDictionary<string, ContextMetaData> moduleContexts;

        public ContextCollectorImpl(IDictionary<string, ContextMetaData> moduleContexts)
        {
            this.moduleContexts = moduleContexts;
        }

        public void RegisterContext(
            string contextName,
            ContextMetaData contextDetail)
        {
            moduleContexts.Put(contextName, contextDetail);
        }
    }
} // end of namespace