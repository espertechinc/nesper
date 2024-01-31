///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethod.cache
{
    public class ExpressionResultCacheForEnumerationMethodImpl : ExpressionResultCacheForEnumerationMethod
    {
        private readonly IDictionary<object, SoftReference<ExpressionResultCacheEntryLongArrayAndObj>> enumMethodCache =
            new IdentityDictionary<object, SoftReference<ExpressionResultCacheEntryLongArrayAndObj>>();

        private Deque<ExpressionResultCacheStackEntry> callStack;
        private Deque<long> lastValueCacheStack;

        public ExpressionResultCacheEntryLongArrayAndObj GetEnumerationMethodLastValue(object node)
        {
            var cacheRef = enumMethodCache.Get(node);

            var entry = cacheRef?.Get();
            if (entry == null) {
                return null;
            }

            var required = entry.Reference;
            if (required.Length != lastValueCacheStack.Count) {
                return null;
            }

            var prov = lastValueCacheStack.GetEnumerator();
            for (var i = 0; i < lastValueCacheStack.Count; i++) {
                prov.MoveNext();
                if (!Equals(required[i], prov.Current)) {
                    return null;
                }
            }

            return entry;
        }

        public void SaveEnumerationMethodLastValue(
            object node,
            object result)
        {
            var snapshot = lastValueCacheStack.ToArray();
            var entry = new ExpressionResultCacheEntryLongArrayAndObj(snapshot, result);
            enumMethodCache.Put(node, new SoftReference<ExpressionResultCacheEntryLongArrayAndObj>(entry));
        }

        public void PushContext(long contextNumber)
        {
            if (callStack == null) {
                callStack = new ArrayDeque<ExpressionResultCacheStackEntry>();
                lastValueCacheStack = new ArrayDeque<long>(10);
            }

            lastValueCacheStack.AddFirst(contextNumber); // Push(contextNumber);
        }

        public void PopContext()
        {
            lastValueCacheStack.RemoveFirst(); //Remove();
        }
    }
} // end of namespace