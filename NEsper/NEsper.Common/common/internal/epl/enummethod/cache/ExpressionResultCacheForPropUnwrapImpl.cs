///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.epl.enummethod.cache
{
    public class ExpressionResultCacheForPropUnwrapImpl : ExpressionResultCacheForPropUnwrap
    {
        private readonly Dictionary<string, SoftReference<ExpressionResultCacheEntryBeanAndCollBean>> collPropertyCache =
            new Dictionary<string, SoftReference<ExpressionResultCacheEntryBeanAndCollBean>>();

        public ExpressionResultCacheEntryBeanAndCollBean GetPropertyColl(
            string propertyNameFullyQualified,
            EventBean reference)
        {
            SoftReference<ExpressionResultCacheEntryBeanAndCollBean> cacheRef = collPropertyCache.Get(propertyNameFullyQualified);
            if (cacheRef == null) {
                return null;
            }

            ExpressionResultCacheEntryBeanAndCollBean entry = cacheRef.Get();
            if (entry == null) {
                return null;
            }

            if (entry.Reference != reference) {
                return null;
            }

            return entry;
        }

        public void SavePropertyColl(
            string propertyNameFullyQualified,
            EventBean reference,
            ICollection<EventBean> events)
        {
            ExpressionResultCacheEntryBeanAndCollBean entry = new ExpressionResultCacheEntryBeanAndCollBean(reference, events);
            collPropertyCache.Put(
                propertyNameFullyQualified,
                new SoftReference<ExpressionResultCacheEntryBeanAndCollBean>(entry));
        }
    }
} // end of namespace