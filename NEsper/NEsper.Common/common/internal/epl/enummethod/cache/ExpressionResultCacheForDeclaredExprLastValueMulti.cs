///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.epl.enummethod.cache
{
    public class ExpressionResultCacheForDeclaredExprLastValueMulti : ExpressionResultCacheForDeclaredExprLastValue
    {
        private readonly int cacheSize;
        private readonly ExpressionResultCacheEntryEventBeanArrayAndObj resultCacheEntry = new ExpressionResultCacheEntryEventBeanArrayAndObj(null, null);

        private readonly Dictionary<object, SoftReference<RollingTwoValueBuffer<EventBean[], object>>> cache
                = new Dictionary<object, SoftReference<RollingTwoValueBuffer<EventBean[], object>>>();

        public ExpressionResultCacheForDeclaredExprLastValueMulti(int cacheSize)
        {
            this.cacheSize = cacheSize;
        }

        public bool CacheEnabled()
        {
            return true;
        }

        public ExpressionResultCacheEntryEventBeanArrayAndObj GetDeclaredExpressionLastValue(object node, EventBean[] eventsPerStream)
        {
            SoftReference<RollingTwoValueBuffer<EventBean[], object>> cacheRef = cache.Get(node);
            if (cacheRef == null)
            {
                return null;
            }
            RollingTwoValueBuffer<EventBean[], object> entry = cacheRef.Get();
            if (entry == null)
            {
                return null;
            }
            for (int i = 0; i < entry.BufferA.Length; i++)
            {
                EventBean[] key = entry.BufferA[i];
                if (key != null && EventBeanUtility.CompareEventReferences(key, eventsPerStream))
                {
                    resultCacheEntry.Reference = key;
                    resultCacheEntry.Result = entry.BufferB[i];
                    return resultCacheEntry;
                }
            }
            return null;
        }

        public void SaveDeclaredExpressionLastValue(object node, EventBean[] eventsPerStream, object result)
        {
            SoftReference<RollingTwoValueBuffer<EventBean[], object>> cacheRef = cache.Get(node);

            RollingTwoValueBuffer<EventBean[], object> buf;
            if (cacheRef == null)
            {
                buf = new RollingTwoValueBuffer<EventBean[], object>(new EventBean[cacheSize][], new object[cacheSize]);
                cache.Put(node, new SoftReference<RollingTwoValueBuffer<EventBean[], object>>(buf));
            }
            else
            {
                buf = cacheRef.Get();
                if (buf == null)
                {
                    buf = new RollingTwoValueBuffer<EventBean[], object>(new EventBean[cacheSize][], new object[cacheSize]);
                    cache.Put(node, new SoftReference<RollingTwoValueBuffer<EventBean[], object>>(buf));
                }
            }

            EventBean[] copy = new EventBean[eventsPerStream.Length];
            Array.Copy(eventsPerStream, 0, copy, 0, copy.Length);
            buf.Add(copy, result);
        }
    }
} // end of namespace