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
	public class JsonEventUnderlyingEntrySetIterator : IEnumerator<KeyValuePair<string, object>> {
	    private readonly JsonEventObjectBase jeu;
	    private readonly IEnumerator<KeyValuePair<string, object>> mapIter;
	    private int count;

	    public JsonEventUnderlyingEntrySetIterator(JsonEventObjectBase jeu, ISet<KeyValuePair<string, object>> entrySet) {
	        this.jeu = jeu;
	        this.mapIter = entrySet.Iterator();
	    }

	    public bool HasNext() {
	        return count < jeu.NativeSize || mapIter.HasNext;
	    }

	    public KeyValuePair<string, object> Next() {
	        if (count < jeu.NativeSize) {
	            return jeu.GetNativeEntry(count++);
	        }
	        return mapIter.Next();
	    }
	}
} // end of namespace
