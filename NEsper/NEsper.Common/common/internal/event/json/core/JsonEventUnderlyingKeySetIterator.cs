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
	public class JsonEventUnderlyingKeySetIterator : IEnumerator<string> {
	    private readonly JsonEventObjectBase jeu;
	    private readonly IEnumerator<string> keyIter;
	    private int count;

	    public JsonEventUnderlyingKeySetIterator(JsonEventObjectBase jeu, ISet<string> keySet) {
	        this.jeu = jeu;
	        this.keyIter = keySet.Iterator();
	    }

	    public bool HasNext() {
	        return count < jeu.NativeSize || keyIter.HasNext;
	    }

	    public string Next() {
	        if (count < jeu.NativeSize) {
	            return jeu.GetNativeKey(count++);
	        }
	        return keyIter.Next();
	    }
	}
} // end of namespace
