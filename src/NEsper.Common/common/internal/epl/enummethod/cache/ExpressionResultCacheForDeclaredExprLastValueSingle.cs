///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public class ExpressionResultCacheForDeclaredExprLastValueSingle : ExpressionResultCacheForDeclaredExprLastValue
    {
        private readonly Dictionary<object, SoftReference<ExpressionResultCacheEntryEventBeanArrayAndObj>>
            exprDeclCacheObject =
                new Dictionary<object, SoftReference<ExpressionResultCacheEntryEventBeanArrayAndObj>>();

        public bool CacheEnabled()
        {
            return true;
        }

        public ExpressionResultCacheEntryEventBeanArrayAndObj GetDeclaredExpressionLastValue(
            object node,
            EventBean[] eventsPerStream)
        {
            var cacheRef = exprDeclCacheObject.Get(node);

            var entry = cacheRef?.Get();
            if (entry == null) {
                return null;
            }

            return EventBeanUtility.CompareEventReferences(entry.Reference, eventsPerStream) ? entry : null;
        }

        public void SaveDeclaredExpressionLastValue(
            object node,
            EventBean[] eventsPerStream,
            object result)
        {
            var copy = EventBeanUtility.CopyArray(eventsPerStream);
            var entry = new ExpressionResultCacheEntryEventBeanArrayAndObj(copy, result);
            exprDeclCacheObject.Put(node, new SoftReference<ExpressionResultCacheEntryEventBeanArrayAndObj>(entry));
        }
    }
} // end of namespace