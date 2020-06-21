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
	public class JsonEventUnderlyingKeySet : ISet<string> {
	    private readonly JsonEventObjectBase jeu;
	    private readonly ISet<string> keySet;

	    public JsonEventUnderlyingKeySet(JsonEventObjectBase jsonEventUnderlyingBase) {
	        this.jeu = jsonEventUnderlyingBase;
	        this.keySet = jeu.JsonValues.KeySet();
	    }

	    public int Size() {
	        return jeu.Count;
	    }

	    public bool IsEmpty() {
	        return jeu.IsEmpty();
	    }

	    public IEnumerator<string> Iterator() {
	        return new JsonEventUnderlyingKeySetIterator(jeu, keySet);
	    }

	    public bool Contains(object value) {
	        return jeu.ContainsKey(value);
	    }

	    public object[] ToArray() {
	        if (jeu.NativeSize == 0) {
	            return keySet.ToArray();
	        }
	        string[] result = new string[Count];
	        FillArray(result);
	        return result;
	    }

	    public bool ContainsAll(ICollection<?> c) {
	        if (jeu.NativeSize == 0) {
	            return keySet.ContainsAll(c);
	        }
	        foreach (object key in c) {
	            if (!Contains(key)) {
	                return false;
	            }
	        }
	        return true;
	    }

	    public <T> T[] ToArray(T[] a) {
	        int nativeSize = jeu.NativeSize;
	        if (nativeSize == 0) {
	            return keySet.ToArray();
	        }
	        string[] array = (string[]) a;
	        int size = Count;
	        if (a.Length >= size) {
	            FillArray(array);
	            return a;
	        }
	        string[] result = new string[Count];
	        FillArray(result);
	        return (T[]) result;
	    }

	    public void Clear() {
	        throw new UnsupportedOperationException();
	    }

	    public bool Remove(object o) {
	        throw new UnsupportedOperationException();
	    }

	    public bool RemoveAll(ICollection<?> c) {
	        throw new UnsupportedOperationException();
	    }

	    public bool RetainAll(ICollection<?> c) {
	        throw new UnsupportedOperationException();
	    }

	    public bool Add(string s) {
	        throw new UnsupportedOperationException();
	    }

	    public bool AddAll(ICollection<? extends string> c) {
	        throw new UnsupportedOperationException();
	    }

	    public bool RemoveIf(Predicate<? super string> filter) {
	        throw new UnsupportedOperationException();
	    }

	    private void FillArray(string[] array) {
	        int size = jeu.NativeSize;
	        for (int i = 0; i < size; i++) {
	            array[i] = jeu.GetNativeKey(i);
	        }
	        IEnumerator<string> it = keySet.Iterator();
	        while (it.HasNext) {
	            array[size++] = it.Next();
	        }
	    }
	}
} // end of namespace
