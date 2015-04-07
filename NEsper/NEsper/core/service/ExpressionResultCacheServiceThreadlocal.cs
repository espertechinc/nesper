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
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.core.service
{
    public class ExpressionResultCacheServiceThreadlocal : ExpressionResultCacheService
    {
        private IThreadLocal<ExpressionResultCacheServiceAgentInstance> _threadCache = ThreadLocalManager.Create(
            () => new ExpressionResultCacheServiceAgentInstance());

        public ExpressionResultCacheServiceThreadlocal()
        {
            Init();
        }

        public void Dispose()
        {
            Init();
        }

        public void Init()
        {
            _threadCache = ThreadLocalManager.Create(
                () => new ExpressionResultCacheServiceAgentInstance());
        }

        public void PushStack(ExpressionResultCacheStackEntry lambda)
        {
            _threadCache.GetOrCreate().PushStack(lambda);
        }

        public bool PopLambda()
        {
            return _threadCache.GetOrCreate().PopLambda();
        }

        public LinkedList<ExpressionResultCacheStackEntry> Stack
        {
            get { return _threadCache.GetOrCreate().Stack; }
        }

        public ExpressionResultCacheEntry<EventBean, ICollection<EventBean>> GetPropertyColl(String propertyNameFullyQualified, EventBean reference)
        {
            return _threadCache.GetOrCreate().GetPropertyColl(propertyNameFullyQualified, reference);
        }

        public void SavePropertyColl(String propertyNameFullyQualified, EventBean reference, ICollection<EventBean> events)
        {
            _threadCache.GetOrCreate().SavePropertyColl(propertyNameFullyQualified, reference, events);
        }

        public ExpressionResultCacheEntry<EventBean[], Object> GetDeclaredExpressionLastValue(Object node, EventBean[] eventsPerStream)
        {
            return _threadCache.GetOrCreate().GetDeclaredExpressionLastValue(node, eventsPerStream);
        }

        public void SaveDeclaredExpressionLastValue(Object node, EventBean[] eventsPerStream, Object result)
        {
            _threadCache.GetOrCreate().SaveDeclaredExpressionLastValue(node, eventsPerStream, result);
        }

        public ExpressionResultCacheEntry<EventBean[], ICollection<EventBean>> GetDeclaredExpressionLastColl(Object node, EventBean[] eventsPerStream)
        {
            return _threadCache.GetOrCreate().GetDeclaredExpressionLastColl(node, eventsPerStream);
        }

        public void SaveDeclaredExpressionLastColl(Object node, EventBean[] eventsPerStream, ICollection<EventBean> result)
        {
            _threadCache.GetOrCreate().SaveDeclaredExpressionLastColl(node, eventsPerStream, result);
        }

        public ExpressionResultCacheEntry<long[], Object> GetEnumerationMethodLastValue(Object node)
        {
            return _threadCache.GetOrCreate().GetEnumerationMethodLastValue(node);
        }

        public void SaveEnumerationMethodLastValue(Object node, Object result)
        {
            _threadCache.GetOrCreate().SaveEnumerationMethodLastValue(node, result);
        }

        public void PushContext(long contextNumber)
        {
            _threadCache.GetOrCreate().PushContext(contextNumber);
        }

        public void PopContext()
        {
            _threadCache.GetOrCreate().PopContext();
        }
    }
}
