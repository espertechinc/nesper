///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Provides 3 caches on the statement-level: 
    /// <para />
    /// (A) On the level of indexed event properties: 
    ///     Properties that are wrapped in EventBean instances, such as for Enumeration Methods, get 
    ///     wrapped only once for the same event. The cache is keyed by property-name and EventBean 
    ///     reference and maintains a Collection&lt;EventBean&gt;.
    /// <para />
    /// (B) On the level of enumeration method:
    ///     If a enumeration method expression is invoked within another enumeration method expression 
    ///     (not counting expression declarations), for example "source.Where(a =&gt; source.MinBy(b =&gt; b.x))" 
    ///     the "source.MinBy(b =&gt; b.x)" is not dependent on any other lambda so the result gets cached. The 
    ///     cache is keyed by the enumeration-method-node as an IdentityHashMap and verified by a context 
    ///     stack (long[]) that is built in nested evaluation calls.
    /// <para /> 
    /// (C) On the level of expression declaration: 
    ///     a) for non-enum evaluation and for enum-evaluation a separate cache 
    ///     b) The cache is keyed by the Prototype-node as an IdentityHashMap and verified by a events-per-stream 
    ///        (EventBean[]) that is maintained or rewritten.
    /// </summary>
    public interface ExpressionResultCacheService
    {
        void PushStack(ExpressionResultCacheStackEntry lambda);
        bool PopLambda();
        LinkedList<ExpressionResultCacheStackEntry> Stack { get; }
        ExpressionResultCacheEntry<EventBean, ICollection<EventBean>> GetPropertyColl(String propertyNameFullyQualified, EventBean reference);
        void SavePropertyColl(String propertyNameFullyQualified, EventBean reference, ICollection<EventBean> events);
        ExpressionResultCacheEntry<EventBean[], Object> GetDeclaredExpressionLastValue(Object node, EventBean[] eventsPerStream);
        void SaveDeclaredExpressionLastValue(Object node, EventBean[] eventsPerStream, Object result);
        ExpressionResultCacheEntry<EventBean[], ICollection<EventBean>> GetDeclaredExpressionLastColl(Object node, EventBean[] eventsPerStream);
        void SaveDeclaredExpressionLastColl(Object node, EventBean[] eventsPerStream, ICollection<EventBean> result);
        ExpressionResultCacheEntry<long[], Object> GetEnumerationMethodLastValue(Object node);
        void SaveEnumerationMethodLastValue(Object node, Object result);
        void PushContext(long contextNumber);
        void PopContext();
    }
}