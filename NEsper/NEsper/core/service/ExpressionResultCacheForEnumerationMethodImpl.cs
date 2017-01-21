///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.core.service
{
    using IdentityCache = IdentityDictionary<object, SoftReference<ExpressionResultCacheEntry<long?[], object>>>;

    public class ExpressionResultCacheForEnumerationMethodImpl : ExpressionResultCacheForEnumerationMethod
    {
        private readonly IdentityCache enumMethodCache = new IdentityCache();

        private Deque<ExpressionResultCacheStackEntry> callStack;
        private Deque<long?> lastValueCacheStack;

        public void PushStack(ExpressionResultCacheStackEntry lambda)
        {
            if (callStack == null)
            {
                callStack = new ArrayDeque<ExpressionResultCacheStackEntry>();
                lastValueCacheStack = new ArrayDeque<long?>(10);
            }
            callStack.AddLast(lambda);
        }

        public bool PopLambda()
        {
            callStack.RemoveLast();
            return callStack.IsEmpty();
        }

        public Deque<ExpressionResultCacheStackEntry> GetStack()
        {
            return callStack;
        }

        public ExpressionResultCacheEntry<long?[], object> GetEnumerationMethodLastValue(object node)
        {
            var cacheRef = enumMethodCache.Get(node);
            if (cacheRef == null)
            {
                return null;
            }
            var entry = cacheRef.Get();
            if (entry == null)
            {
                return null;
            }
            var required = entry.Reference;
            if (required.Length != lastValueCacheStack.Count)
            {
                return null;
            }
            var prov = lastValueCacheStack.GetEnumerator();
            for (int i = 0; i < lastValueCacheStack.Count; i++)
            {
                prov.MoveNext();
                if (!required[i].Equals(prov.Current))
                {
                    return null;
                }
            }
            return entry;
        }

        public void SaveEnumerationMethodLastValue(object node, object result)
        {
            long?[] snapshot = lastValueCacheStack.ToArray();
            var entry = new ExpressionResultCacheEntry<long?[], object>(snapshot, result);
            enumMethodCache.Put(node, new SoftReference<ExpressionResultCacheEntry<long?[], object>>(entry));
        }

        public void PushContext(long contextNumber)
        {
            if (callStack == null)
            {
                callStack = new ArrayDeque<ExpressionResultCacheStackEntry>();
                lastValueCacheStack = new ArrayDeque<long?>(10);
            }
            lastValueCacheStack.AddLast(contextNumber);
        }

        public void PopContext()
        {
            lastValueCacheStack.RemoveLast();
        }
    }
} // end of namespace
