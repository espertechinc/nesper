///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.epl.join.exec.composite;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.index.composite
{
    /// <summary>
    /// For use when the index comprises of either two or more ranges or a unique key in combination with a range.
    /// Organizes into a TreeMap&lt;key, TreeMap&lt;key2, Set&lt;EventBean&gt;&gt;&gt;, for short. The top level
    /// can also be just Map&lt;HashableMultiKey, TreeMap...&gt;.
    ///
    /// Expected at least either (A) one key and one range or (B) zero keys and 2 ranges.
    ///
    /// <para>
    /// An alternative implementation could have been based on "TreeMap&lt;ComparableMultiKey, Set&lt;EventBean&gt;&gt;",
    /// however the following implication arrive
    /// - not applicable for range-only lookups (since there the key can be the value itself
    /// - not applicable for multiple nested range as ordering not nested
    /// - each add/remove and lookup would also need to construct a key object.
    /// </para>
    /// </summary>
    public class PropertyCompositeEventTableImpl : PropertyCompositeEventTable
    {
        /// <summary>
        /// Index table (sorted and/or keyed, always nested).
        /// </summary>
        private readonly IDictionary<object, CompositeIndexEntry> _index;

        public PropertyCompositeEventTableImpl(PropertyCompositeEventTableFactory factory)
            : base(factory)
        {
            if (factory.HashGetter != null) {
                var comparer = new AsymmetricEqualityComparer();
                _index = new Dictionary<object, CompositeIndexEntry>(comparer)
                    .WithNullKeySupport();
            }
            else {
                _index = new OrderedListDictionary<object, CompositeIndexEntry>();
            }
        }

        public override IDictionary<object, CompositeIndexEntry> Index => _index;

        public override void Add(
            EventBean theEvent,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            factory.Chain.Enter(theEvent, _index);
        }

        public override void Remove(
            EventBean theEvent,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            factory.Chain.Remove(theEvent, _index);
        }

        public override bool IsEmpty => _index.IsEmpty();

        public override IEnumerator<EventBean> GetEnumerator()
        {
            ISet<EventBean> result = new LinkedHashSet<EventBean>();
            factory.Chain.GetAll(result, _index);
            return result.GetEnumerator();
        }

        public override void Clear()
        {
            _index.Clear();
        }

        public override void Destroy()
        {
            Clear();
        }

        public override int NumKeys => _index.Count;

        public override Type ProviderClass => typeof(PropertyCompositeEventTable);

        public override CompositeIndexQueryResultPostProcessor PostProcessor => null;
    }
} // end of namespace