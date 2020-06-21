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

using java.util.function;


namespace com.espertech.esper.common.@internal.@event.json.core
{
	public class JsonEventUnderlyingValueCollection : ICollection<object> {

	    private readonly JsonEventObjectBase jeu;
	    private readonly ICollection<object> values;

	    public JsonEventUnderlyingValueCollection(JsonEventObjectBase jeu, ICollection<object> values) {
	        this.jeu = jeu;
	        this.values = values;
	    }

	    public int Size() {
	        return jeu.Count;
	    }

	    public bool IsEmpty() {
	        return jeu.IsEmpty();
	    }

	    public object[] ToArray() {
	        object[] result = new object[Count];
	        FillArray(result);
	        return result;
	    }

	    public IEnumerator<object> Iterator() {
	        return new JsonEventUnderlyingValueIterator(jeu, values.Iterator());
	    }

	    public bool Contains(object value) {
	        if (value == null) {
	            for (int i = 0; i < jeu.NativeSize; i++) {
	                if (jeu.GetNativeValue(i) == null) {
	                    return true;
	                }
	            }
	        } else {
	            for (int i = 0; i < jeu.NativeSize; i++) {
	                if (value.Equals(jeu.GetNativeValue(i))) {
	                    return true;
	                }
	            }
	        }
	        return values.Contains(value);
	    }

	    public bool ContainsAll(ICollection<?> c) {
	        foreach (object e in c) {
	            if (!Contains(e)) {
	                return false;
	            }
	        }
	        return true;
	    }

	    public <T> T[] ToArray(T[] array) {
	        int nativeSize = jeu.NativeSize;
	        if (nativeSize == 0) {
	            return values.ToArray();
	        }
	        int size = Count;
	        if (array.Length >= size) {
	            FillArray(array);
	            return array;
	        }
	        object[] result = new object[Count];
	        FillArray(result);
	        return (T[]) result;
	    }

	    public bool Add(object o) {
	        throw new UnsupportedOperationException();
	    }

	    public bool Remove(object o) {
	        throw new UnsupportedOperationException();
	    }

	    public bool AddAll(ICollection<?> c) {
	        throw new UnsupportedOperationException();
	    }

	    public bool RemoveAll(ICollection<?> c) {
	        throw new UnsupportedOperationException();
	    }

	    public bool RetainAll(ICollection<?> c) {
	        throw new UnsupportedOperationException();
	    }

	    public void Clear() {
	        throw new UnsupportedOperationException();
	    }

	    public bool RemoveIf(Predicate<? super object> filter) {
	        throw new UnsupportedOperationException();
	    }

	    private void FillArray(object[] array) {
	        int size = jeu.NativeSize;
	        for (int i = 0; i < size; i++) {
	            array[i] = jeu.GetNativeValue(i);
	        }
	        IEnumerator<object> it = values.Iterator();
	        while (it.HasNext) {
	            array[size++] = it.Next();
	        }
	    }
	}
} // end of namespace
