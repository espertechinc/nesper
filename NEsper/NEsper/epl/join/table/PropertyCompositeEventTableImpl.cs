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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.exec.composite;

namespace com.espertech.esper.epl.join.table
{
	/// <summary>
	/// For use when the index comprises of either two or more ranges or a unique key in combination with a range.
	/// Expected at least either (A) one key and one range or (B) zero keys and 2 ranges.
	/// <para />- not applicable for range-only lookups (since there the key can be the value itself
	/// - not applicable for multiple nested range as ordering not nested
	/// - each add/remove and lookup would also need to construct a key object.
	/// </summary>
	public class PropertyCompositeEventTableImpl : PropertyCompositeEventTable
	{
	    private readonly CompositeIndexEnterRemove _chain;

	    /// <summary>
	    /// Index table (sorted and/or keyed, always nested).
	    /// </summary>
	    private readonly IDictionary<object, object> _index;

	    public PropertyCompositeEventTableImpl(IList<Type> optKeyCoercedTypes, IList<Type> optRangeCoercedTypes, EventTableOrganization organization, bool isHashKeyed, CompositeIndexEnterRemove chain)
	        : base(optKeyCoercedTypes, optRangeCoercedTypes, organization)
	    {
	        _chain = chain;
	        _index = isHashKeyed
	            ? (IDictionary<object, object>) new Dictionary<object, object>()
                : (IDictionary<object, object>) new OrderedDictionary<object, object>();
	    }

	    public override object Index
	    {
	        get { return _index; }
	    }

	    public override IDictionary<object, object> IndexTable
	    {
	        get { return _index; }
	    }

	    public override void Add(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        _chain.Enter(theEvent, _index);
	    }

	    public override void Remove(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        _chain.Remove(theEvent, _index);
	    }

	    public override bool IsEmpty()
	    {
	        return _index.IsEmpty();
	    }

	    public override IEnumerator<EventBean> GetEnumerator()
	    {
	        var result = new LinkedHashSet<EventBean>();
	        _chain.GetAll(result, _index);
	        return result.GetEnumerator();
	    }

	    public override void Clear()
	    {
	        _index.Clear();
	    }

	    public override void Destroy() {
	        Clear();
	    }

	    public override int NumKeys
	    {
	        get { return _index.Count; }
	    }

	    public override Type ProviderClass
	    {
	        get { return typeof (PropertyCompositeEventTable); }
	    }

	    public override CompositeIndexQueryResultPostProcessor PostProcessor
	    {
	        get { return null; }
	    }
	}
} // end of namespace
