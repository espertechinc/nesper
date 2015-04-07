///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.service
{
    public class ExpressionResultCacheServiceAgentInstance : ExpressionResultCacheService
    {
        private Dictionary<String, compat.WeakReference<ExpressionResultCacheEntry<EventBean, ICollection<EventBean>>>> _collPropertyCache;
        private IdentityDictionary<Object, compat.WeakReference<ExpressionResultCacheEntry<EventBean[], Object>>> _exprDeclCacheObject;
        private IdentityDictionary<Object, compat.WeakReference<ExpressionResultCacheEntry<EventBean[], ICollection<EventBean>>>> _exprDeclCacheCollection;
        private IdentityDictionary<Object, compat.WeakReference<ExpressionResultCacheEntry<long[], Object>>> _enumMethodCache;

        private LinkedList<ExpressionResultCacheStackEntry> _callStack;
        private LinkedList<long> _lastValueCacheStack;

        public void PushStack(ExpressionResultCacheStackEntry lambda)
        {
            if (_callStack == null)
            {
                _callStack = new LinkedList<ExpressionResultCacheStackEntry>();
                _lastValueCacheStack = new LinkedList<long>();
            }
            _callStack.AddLast(lambda);
        }

        public bool PopLambda()
        {
            _callStack.RemoveLast();
            return _callStack.IsEmpty();
        }

        public LinkedList<ExpressionResultCacheStackEntry> Stack
        {
            get { return _callStack; }
        }

        public void Dispose()
        {
        }

        public ExpressionResultCacheEntry<EventBean, ICollection<EventBean>> GetPropertyColl(String propertyNameFullyQualified, EventBean reference)
        {
            InitPropertyCollCache();
            var cacheRef = _collPropertyCache.Get(propertyNameFullyQualified);
            if (cacheRef == null)
            {
                return null;
            }
            var entry = cacheRef.Target;
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

        public void SavePropertyColl(String propertyNameFullyQualified, EventBean reference, ICollection<EventBean> events)
        {
            var entry = new ExpressionResultCacheEntry<EventBean, ICollection<EventBean>>(reference, events);
            _collPropertyCache.Put(propertyNameFullyQualified, new compat.WeakReference<ExpressionResultCacheEntry<EventBean, ICollection<EventBean>>>(entry));
        }

        public ExpressionResultCacheEntry<EventBean[], Object> GetDeclaredExpressionLastValue(Object node, EventBean[] eventsPerStream)
        {
            InitExprDeclaredCacheObject();
            var cacheRef = _exprDeclCacheObject.Get(node);
            if (cacheRef == null)
            {
                return null;
            }
            var entry = cacheRef.Target;
            if (entry == null)
            {
                return null;
            }
            var cacheEvents = entry.Reference;
            if (cacheEvents.Length != eventsPerStream.Length)
            {
                return null;
            }
            for (int i = 0; i < cacheEvents.Length; i++)
            {
                if (cacheEvents[i] != eventsPerStream[i])
                {
                    return null;
                }
            }
            return entry;
        }

        public void SaveDeclaredExpressionLastValue(Object node, EventBean[] eventsPerStream, Object result)
        {
            var copy = new EventBean[eventsPerStream.Length];
            Array.Copy(eventsPerStream, 0, copy, 0, copy.Length);
            var entry = new ExpressionResultCacheEntry<EventBean[], Object>(copy, result);
            _exprDeclCacheObject.Put(node, new compat.WeakReference<ExpressionResultCacheEntry<EventBean[], Object>>(entry));
        }

        public ExpressionResultCacheEntry<EventBean[], ICollection<EventBean>> GetDeclaredExpressionLastColl(Object node, EventBean[] eventsPerStream)
        {
            InitExprDeclaredCacheCollection();
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
            var cacheEvents = entry.Reference;
            if (cacheEvents.Length != eventsPerStream.Length)
            {
                return null;
            }
            for (int i = 0; i < cacheEvents.Length; i++)
            {
                if (cacheEvents[i] != eventsPerStream[i])
                {
                    return null;
                }
            }
            return entry;
        }

        public void SaveDeclaredExpressionLastColl(Object node, EventBean[] eventsPerStream, ICollection<EventBean> result)
        {
            var copy = new EventBean[eventsPerStream.Length];
            Array.Copy(eventsPerStream, 0, copy, 0, copy.Length);
            var entry = new ExpressionResultCacheEntry<EventBean[], ICollection<EventBean>>(copy, result);
            _exprDeclCacheCollection.Put(node, new compat.WeakReference<ExpressionResultCacheEntry<EventBean[], ICollection<EventBean>>>(entry));
        }

        public ExpressionResultCacheEntry<long[], Object> GetEnumerationMethodLastValue(Object node)
        {
            InitEnumMethodCache();
            var cacheRef = _enumMethodCache.Get(node);
            if (cacheRef == null)
            {
                return null;
            }
            ExpressionResultCacheEntry<long[], Object> entry = cacheRef.Target;
            if (entry == null)
            {
                return null;
            }
            long[] required = entry.Reference;
            if (required.Length != _lastValueCacheStack.Count)
            {
                return null;
            }
            IEnumerator<long> prov = _lastValueCacheStack.GetEnumerator();
            for (int i = 0; i < _lastValueCacheStack.Count; i++)
            {
                prov.MoveNext();
                if (!required[i].Equals(prov.Current))
                {
                    return null;
                }
            }
            return entry;
        }

        public void SaveEnumerationMethodLastValue(Object node, Object result)
        {
            var snapshot = _lastValueCacheStack.ToArray();
            var entry = new ExpressionResultCacheEntry<long[], Object>(snapshot, result);
            _enumMethodCache.Put(node, new compat.WeakReference<ExpressionResultCacheEntry<long[], Object>>(entry));
        }

        private void InitEnumMethodCache()
        {
            if (_enumMethodCache == null)
            {
                _enumMethodCache = new IdentityDictionary<Object, compat.WeakReference<ExpressionResultCacheEntry<long[], Object>>>();
            }
        }

        private void InitPropertyCollCache()
        {
            if (_collPropertyCache == null)
            {
                _collPropertyCache = new Dictionary<String, compat.WeakReference<ExpressionResultCacheEntry<EventBean, ICollection<EventBean>>>>();
            }
        }

        private void InitExprDeclaredCacheObject()
        {
            if (_exprDeclCacheObject == null)
            {
                _exprDeclCacheObject = new IdentityDictionary<Object, compat.WeakReference<ExpressionResultCacheEntry<EventBean[], Object>>>();
            }
        }

        private void InitExprDeclaredCacheCollection()
        {
            if (_exprDeclCacheCollection == null)
            {
                _exprDeclCacheCollection = new IdentityDictionary<Object, compat.WeakReference<ExpressionResultCacheEntry<EventBean[], ICollection<EventBean>>>>();
            }
        }

        public void PushContext(long contextNumber)
        {
            if (_callStack == null)
            {
                _callStack = new LinkedList<ExpressionResultCacheStackEntry>();
                _lastValueCacheStack = new LinkedList<long>();
            }
            _lastValueCacheStack.AddLast(contextNumber);
        }

        public void PopContext()
        {
            _lastValueCacheStack.RemoveLast();
        }
    }
}
