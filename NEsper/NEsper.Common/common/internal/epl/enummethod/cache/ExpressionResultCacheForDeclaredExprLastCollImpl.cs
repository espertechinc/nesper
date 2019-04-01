///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethod.cache
{
    using IdentityMap = IdentityDictionary<object, SoftReference<ExpressionResultCacheEntryEventBeanArrayAndCollBean>>;

    public class ExpressionResultCacheForDeclaredExprLastCollImpl : ExpressionResultCacheForDeclaredExprLastColl
    {
        private readonly IdentityMap exprDeclCacheCollection = new IdentityMap();

        public ExpressionResultCacheEntryEventBeanArrayAndCollBean GetDeclaredExpressionLastColl(
            object node, EventBean[] eventsPerStream)
        {
            var cacheRef = exprDeclCacheCollection.Get(node);
            if (cacheRef == null) {
                return null;
            }

            var entry = cacheRef.Get();
            if (entry == null) {
                return null;
            }

            return EventBeanUtility.CompareEventReferences(entry.Reference, eventsPerStream) ? entry : null;
        }

        public void SaveDeclaredExpressionLastColl(
            object node, EventBean[] eventsPerStream, ICollection<EventBean> result)
        {
            var copy = EventBeanUtility.CopyArray(eventsPerStream);
            var entry = new ExpressionResultCacheEntryEventBeanArrayAndCollBean(copy, result);
            exprDeclCacheCollection.Put(
                node, new SoftReference<ExpressionResultCacheEntryEventBeanArrayAndCollBean>(entry));
        }
    }
} // end of namespace