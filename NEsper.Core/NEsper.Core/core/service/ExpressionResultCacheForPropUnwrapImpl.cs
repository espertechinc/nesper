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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.core.service
{
    using Cache = Dictionary<string, SoftReference<ExpressionResultCacheEntry<EventBean, ICollection<EventBean>>>>;

    public class ExpressionResultCacheForPropUnwrapImpl : ExpressionResultCacheForPropUnwrap
    {
        private readonly Cache _collPropertyCache = new Cache();

        public ExpressionResultCacheEntry<EventBean, ICollection<EventBean>> GetPropertyColl(
            string propertyNameFullyQualified,
            EventBean reference)
        {
            var cacheRef = _collPropertyCache.Get(propertyNameFullyQualified);
            if (cacheRef == null)
            {
                return null;
            }
            var entry = cacheRef.Get();
            if (entry == null)
            {
                return null;
            }
            if (entry.Reference != reference)
            {
                return null;
            }
            return entry;
        }

        public void SavePropertyColl(
            string propertyNameFullyQualified,
            EventBean reference,
            ICollection<EventBean> events)
        {
            var entry = new ExpressionResultCacheEntry<EventBean, ICollection<EventBean>>(reference, events);
            _collPropertyCache.Put(
                propertyNameFullyQualified,
                new SoftReference<ExpressionResultCacheEntry<EventBean, ICollection<EventBean>>>(entry));
        }
    }
} // end of namespace
