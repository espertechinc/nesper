///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.core.context.util
{
    public class ContextDescriptor
    {
        private readonly string _contextName;
        private readonly bool _singleInstanceContext;
        private readonly ContextPropertyRegistry _contextPropertyRegistry;
        private readonly StatementAIResourceRegistryFactory _aiResourceRegistryFactory;
        private readonly ContextEnumeratorHandler _iteratorHandler;
        private readonly ContextDetail _contextDetail;

        public ContextDescriptor(string contextName, bool singleInstanceContext, ContextPropertyRegistry contextPropertyRegistry, StatementAIResourceRegistryFactory aiResourceRegistryFactory, ContextEnumeratorHandler iteratorHandler, ContextDetail contextDetail)
        {
            _contextName = contextName;
            _singleInstanceContext = singleInstanceContext;
            _contextPropertyRegistry = contextPropertyRegistry;
            _aiResourceRegistryFactory = aiResourceRegistryFactory;
            _iteratorHandler = iteratorHandler;
            _contextDetail = contextDetail;
        }

        public string ContextName
        {
            get { return _contextName; }
        }

        public bool IsSingleInstanceContext
        {
            get { return _singleInstanceContext; }
        }

        public ContextPropertyRegistry ContextPropertyRegistry
        {
            get { return _contextPropertyRegistry; }
        }

        public StatementAIResourceRegistryFactory AiResourceRegistryFactory
        {
            get { return _aiResourceRegistryFactory; }
        }

        public IEnumerator<EventBean> GetEnumerator(int statementId) {
            return _iteratorHandler.GetEnumerator(statementId);
        }

        public IEnumerator<EventBean> GetSafeEnumerator(int statementId)
        {
            return _iteratorHandler.GetSafeEnumerator(statementId);
        }

        public IEnumerator<EventBean> GetEnumerator(int statementId, ContextPartitionSelector selector)
        {
            return _iteratorHandler.GetEnumerator(statementId, selector);
        }

        public IEnumerator<EventBean> GetSafeEnumerator(int statementId, ContextPartitionSelector selector)
        {
            return _iteratorHandler.GetSafeEnumerator(statementId, selector);
        }

        public ContextDetail ContextDetail
        {
            get { return _contextDetail; }
        }
    }
}
