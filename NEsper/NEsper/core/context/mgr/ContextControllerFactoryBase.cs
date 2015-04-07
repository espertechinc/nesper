///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.core.context.stmt;
using com.espertech.esper.epl.spec;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.context.mgr
{
    public abstract class ContextControllerFactoryBase : ContextControllerFactory
    {
        private readonly ContextControllerFactoryContext _factoryContext;

        protected ContextControllerFactoryBase(ContextControllerFactoryContext factoryContext)
        {
            _factoryContext = factoryContext;
        }

        public virtual ContextControllerFactoryContext FactoryContext
        {
            get { return _factoryContext; }
        }

        public abstract IDictionary<string, object> ContextBuiltinProps { get; }
        public abstract bool IsSingleInstanceContext { get; }
        public abstract ContextDetail ContextDetail { get; }
        public abstract IList<ContextDetailPartitionItem> ContextDetailPartitionItems { get; }
        public abstract StatementAIResourceRegistryFactory StatementAIResourceRegistryFactory { get; }
        public abstract void ValidateFactory();
        public abstract ContextControllerStatementCtxCache ValidateStatement(ContextControllerStatementBase statement);
        public abstract ContextController CreateNoCallback(int pathId, ContextControllerLifecycleCallback callback);
        public abstract void PopulateFilterAddendums(IDictionary<FilterSpecCompiled, FilterValueSetParam[][]> filterAddendum, ContextControllerStatementDesc statement, object key, int contextId);
        public abstract FilterSpecLookupable GetFilterLookupable(EventType eventType);
        public abstract ContextStateCache StateCache { get; }
        public abstract ContextPartitionIdentifier KeyPayloadToIdentifier(object payload);
    }
}
