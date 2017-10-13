///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events;

namespace com.espertech.esper.core.service
{
    using IdentityCache = IdentityDictionary<object, SoftReference<ExpressionResultCacheEntry<EventBean[], ICollection<EventBean>>>>;

    public class ExpressionResultCacheForDeclaredExprLastCollImpl : ExpressionResultCacheForDeclaredExprLastColl
    {
        private readonly IdentityCache _exprDeclCacheCollection = new IdentityCache();

        public ExpressionResultCacheEntry<EventBean[], ICollection<EventBean>> GetDeclaredExpressionLastColl(object node, EventBean[] eventsPerStream)
        {
            var cacheRef = _exprDeclCacheCollection.Get(node);
            if (cacheRef == null)
            {
                return null;
            }
            
            var entry = cacheRef.Target;
            if (entry == null)
            {
                return null;
            }
            return EventBeanUtility.CompareEventReferences(entry.Reference, eventsPerStream) ? entry : null;
        }

        public void SaveDeclaredExpressionLastColl(object node, EventBean[] eventsPerStream, ICollection<EventBean> result)
        {
            var copy = eventsPerStream.MaterializeArray();
            var entry = new ExpressionResultCacheEntry<EventBean[], ICollection<EventBean>>(copy, result);
            _exprDeclCacheCollection.Put(
                node, new SoftReference<ExpressionResultCacheEntry<EventBean[], ICollection<EventBean>>>(entry));
        }
    }
} // end of namespace
