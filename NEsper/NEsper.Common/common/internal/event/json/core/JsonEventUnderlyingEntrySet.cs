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
	public class JsonEventUnderlyingEntrySet : ISet<KeyValuePair<string, object>> {
	    private readonly JsonEventObjectBase jeu;
	    private readonly ISet<KeyValuePair<string, object>> entrySet;

	    public JsonEventUnderlyingEntrySet(JsonEventObjectBase jeu) {
	        this.jeu = jeu;
	        this.entrySet = jeu.JsonValues.EntrySet();
	    }

	    public int Size() {
	        return jeu.Count;
	    }

	    public IEnumerator<KeyValuePair<string, object>> Iterator() {
	        return new JsonEventUnderlyingEntrySetIterator(jeu, entrySet);
	    }

	    public bool IsEmpty() {
	        return jeu.NativeSize == 0 && jeu.JsonValues.IsEmpty();
	    }

	    public object[] ToArray() {
	        KeyValuePair[] result = new IDictionary.Entry[Count];
	        FillArray(result);
	        return result;
	    }

	    public bool Contains(object o) {
	        IEnumerator<KeyValuePair<string, object>> it = Iterator();
	        if (o == null) {
	            while (it.HasNext) {
	                if (it.Next() == null) {
	                    return true;
	                }
	            }
	        } else {
	            while (it.HasNext) {
	                if (o.Equals(it.Next())) {
	                    return true;
	                }
	            }
	        }
	        return false;
	    }

	    public T[] ToArray<T>(T[] a) {
	        int nativeSize = jeu.NativeSize;
	        if (nativeSize == 0) {
	            return entrySet.ToArray();
	        }
	        KeyValuePair[] array = (KeyValuePair[]) a;
	        int size = Count;
	        if (a.Length >= size) {
	            FillArray(array);
	            return a;
	        }
	        KeyValuePair[] result = new IDictionary.Entry[Count];
	        FillArray(result);
	        return (T[]) result;
	    }

	    public bool ContainsAll(ICollection<?> c) {
	        if (jeu.NativeSize == 0) {
	            return entrySet.ContainsAll(c);
	        }
	        foreach (object key in c) {
	            if (!Contains(key)) {
	                return false;
	            }
	        }
	        return true;
	    }

	    public bool Add(KeyValuePair<string, object> stringObjectEntry) {
	        throw new UnsupportedOperationException();
	    }

	    public bool Remove(object o) {
	        throw new UnsupportedOperationException();
	    }

	    public bool AddAll(ICollection<? extends KeyValuePair<string, object>> c) {
	        throw new UnsupportedOperationException();
	    }

	    public bool RetainAll(ICollection<?> c) {
	        throw new UnsupportedOperationException();
	    }

	    public bool RemoveAll(ICollection<?> c) {
	        throw new UnsupportedOperationException();
	    }

	    public void Clear() {
	        throw new UnsupportedOperationException();
	    }

	    public bool RemoveIf(Predicate<KeyValuePair<string, object>> filter) {
	        throw new UnsupportedOperationException();
	    }

	    private void FillArray(KeyValuePair[] result) {
	        int size = jeu.NativeSize;
	        for (int i = 0; i < size; i++) {
	            result[i] = jeu.GetNativeEntry(i);
	        }
	        IEnumerator<KeyValuePair<string, object>> it = entrySet.Iterator();
	        while (it.HasNext) {
	            result[size++] = it.Next();
	        }
	    }
	}
} // end of namespace
