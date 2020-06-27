///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.@event.json.core
{
	public class JsonEventUnderlyingValueCollection : ICollection<object> {

	    private readonly JsonEventObjectBase _underlyingBase;

	    public JsonEventUnderlyingValueCollection(JsonEventObjectBase underlyingBase)
	    {
		    this._underlyingBase = underlyingBase;
	    }

	    public int Count => _underlyingBase.Count;

	    public bool IsReadOnly => true;

	    public void Add(object item)
	    {
		    throw new UnsupportedOperationException();
	    }

	    public bool Remove(object item)
	    {
		    throw new UnsupportedOperationException();
	    }

	    public void Clear()
	    {
		    throw new UnsupportedOperationException();
	    }

	    public bool Contains(object value)
	    {
		    return _underlyingBase.ContainsValue(value);
	    }

	    IEnumerator IEnumerable.GetEnumerator()
	    {
		    return GetEnumerator();
	    }

	    public IEnumerator<object> GetEnumerator()
	    {
		    return _underlyingBase
			    .Select(entry => entry.Value)
			    .GetEnumerator();
	    }

	    public void CopyTo(
		    object[] array,
		    int arrayIndex)
	    {
		    var arrayLength = array.Length;

		    using(var enumerator = _underlyingBase.NativeEnumerable().GetEnumerator()) {
			    while ((arrayIndex < arrayLength) && enumerator.MoveNext()) {
				    array[arrayIndex] = enumerator.Current.Value;
				    arrayIndex++;
			    }
		    }

		    using (var enumerator = _underlyingBase.JsonValues.GetEnumerator()) {
			    while ((arrayIndex < arrayLength) && enumerator.MoveNext()) {
				    array[arrayIndex] = enumerator.Current.Value;
				    arrayIndex++;
			    }
		    }
	    }
	}
} // end of namespace
