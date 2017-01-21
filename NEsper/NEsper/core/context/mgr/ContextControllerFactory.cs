///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.core.context.stmt;
using com.espertech.esper.epl.spec;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.context.mgr
{
    public interface ContextControllerFactory
    {
        ContextControllerFactoryContext FactoryContext { get; }

        IDictionary<string, object> ContextBuiltinProps { get; }
        bool IsSingleInstanceContext { get; }
        ContextDetail ContextDetail { get; }
        IList<ContextDetailPartitionItem> ContextDetailPartitionItems { get; }
        StatementAIResourceRegistryFactory StatementAIResourceRegistryFactory { get; }

        void ValidateFactory();
        ContextControllerStatementCtxCache ValidateStatement(ContextControllerStatementBase statement);
        ContextController CreateNoCallback(int pathId, ContextControllerLifecycleCallback callback);
        void PopulateFilterAddendums(IDictionary<FilterSpecCompiled, FilterValueSetParam[][]> filterAddendum, ContextControllerStatementDesc statement, Object key, int contextId);
    
        FilterSpecLookupable GetFilterLookupable(EventType eventType);

        ContextStateCache StateCache { get; }

        ContextPartitionIdentifier KeyPayloadToIdentifier(Object payload);
    }
}
