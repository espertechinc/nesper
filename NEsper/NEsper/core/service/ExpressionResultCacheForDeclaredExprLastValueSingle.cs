///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events;

namespace com.espertech.esper.core.service
{
    using IdentityCache = IdentityDictionary<object, SoftReference<ExpressionResultCacheEntry<EventBean[], object>>>;

    public class ExpressionResultCacheForDeclaredExprLastValueSingle : ExpressionResultCacheForDeclaredExprLastValue
    {
        private readonly IdentityCache _exprDeclCacheObject = new IdentityCache();

        public bool CacheEnabled()
        {
            return true;
        }

        public ExpressionResultCacheEntry<EventBean[], object> GetDeclaredExpressionLastValue(object node, EventBean[] eventsPerStream)
        {
            var cacheRef = this._exprDeclCacheObject.Get(
                node);
            if (cacheRef == null)
            {
                return null;
            }
            var entry = cacheRef.Get();
            if (entry == null)
            {
                return null;
            }
            return EventBeanUtility.CompareEventReferences(entry.Reference, eventsPerStream) ? entry : null;
        }

        public void SaveDeclaredExpressionLastValue(object node, EventBean[] eventsPerStream, object result)
        {
            EventBean[] copy = EventBeanUtility.CopyArray(eventsPerStream);
            var entry = new ExpressionResultCacheEntry<EventBean[], object>(copy, result);
            _exprDeclCacheObject.Put(node, new SoftReference<ExpressionResultCacheEntry<EventBean[], object>>(entry));
        }
    }
} // end of namespace
