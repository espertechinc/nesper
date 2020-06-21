///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.@event.json.core
{
	public class JsonEventUnderlyingValueIterator : IEnumerator<object> {
	    private readonly JsonEventObjectBase jeu;
	    private readonly IEnumerator<object> valuesIter;
	    private int count;

	    public JsonEventUnderlyingValueIterator(JsonEventObjectBase jeu, IEnumerator<object> valuesIter) {
	        this.jeu = jeu;
	        this.valuesIter = valuesIter;
	    }

	    public bool HasNext() {
	        return count < jeu.NativeSize || valuesIter.HasNext;
	    }

	    public object Next() {
	        if (count < jeu.NativeSize) {
	            return jeu.GetNativeValue(count++);
	        }
	        return valuesIter.Next();
	    }
	}
} // end of namespace
