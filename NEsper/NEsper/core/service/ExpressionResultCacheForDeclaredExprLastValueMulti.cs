///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events;

namespace com.espertech.esper.core.service
{
    using IdentityCache = IdentityDictionary<object, SoftReference<RollingTwoValueBuffer<EventBean[], object>>>;

    public class ExpressionResultCacheForDeclaredExprLastValueMulti : ExpressionResultCacheForDeclaredExprLastValue
    {
        private readonly int _cacheSize;

        private readonly ExpressionResultCacheEntry<EventBean[], object> _resultCacheEntry =
            new ExpressionResultCacheEntry<EventBean[], object>(null, null);

        private readonly IdentityCache _cache = new IdentityCache();

        public ExpressionResultCacheForDeclaredExprLastValueMulti(int cacheSize)
        {
            _cacheSize = cacheSize;
        }

        public bool CacheEnabled()
        {
            return true;
        }

        public ExpressionResultCacheEntry<EventBean[], object> GetDeclaredExpressionLastValue(object node, EventBean[] eventsPerStream)
        {
            var cacheRef = _cache.Get(node);
            if (cacheRef == null)
            {
                return null;
            }

            var entry = cacheRef.Get();
            if (entry == null)
            {
                return null;
            }
            for (var i = 0; i < entry.BufferA.Length; i++)
            {
                var key = entry.BufferA[i];
                if (key != null && EventBeanUtility.CompareEventReferences(key, eventsPerStream))
                {
                    _resultCacheEntry.Reference = key;
                    _resultCacheEntry.Result = entry.BufferB[i];
                    return _resultCacheEntry;
                }
            }
            return null;
        }

        public void SaveDeclaredExpressionLastValue(object node, EventBean[] eventsPerStream, object result)
        {
            var cacheRef = _cache.Get(node);

            RollingTwoValueBuffer<EventBean[], object> buf;
            if (cacheRef == null)
            {
                buf = new RollingTwoValueBuffer<EventBean[], object>(new EventBean[_cacheSize][], new object[_cacheSize]);
                _cache.Put(node, new SoftReference<RollingTwoValueBuffer<EventBean[], object>>(buf));
            }
            else
            {
                buf = cacheRef.Get();
                if (buf == null)
                {
                    buf = new RollingTwoValueBuffer<EventBean[], object>(new EventBean[_cacheSize][], new object[_cacheSize]);
                    _cache.Put(node, new SoftReference<RollingTwoValueBuffer<EventBean[], object>>(buf));
                }
            }

            var copy = new EventBean[eventsPerStream.Length];
            Array.Copy(eventsPerStream, 0, copy, 0, copy.Length);
            buf.Add(copy, result);
        }
    }
} // end of namespace
