///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public class JsonEventUnderlyingKeyCollection : ICollection<string>
    {
        private readonly JsonEventObjectBase _underlyingBase;

        public JsonEventUnderlyingKeyCollection(JsonEventObjectBase jsonEventUnderlyingBase)
        {
            _underlyingBase = jsonEventUnderlyingBase;
        }

        public int Count => _underlyingBase.Count;

        public bool IsReadOnly => true;

        public void Add(string item)
        {
            throw new UnsupportedOperationException();
        }

        public bool Remove(string item)
        {
            throw new UnsupportedOperationException();
        }

        public void Clear()
        {
            throw new UnsupportedOperationException();
        }

        public bool Contains(string value)
        {
            return _underlyingBase.ContainsKey(value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _underlyingBase
                .Select(entry => entry.Key)
                .GetEnumerator();
        }

        public void CopyTo(
            string[] array,
            int arrayIndex)
        {
            var arrayLength = array.Length;

            using (var enumerator = _underlyingBase.NativeEnumerable.GetEnumerator()) {
                while (arrayIndex < arrayLength && enumerator.MoveNext()) {
                    array[arrayIndex] = _underlyingBase.GetNativeKeyName(enumerator.Current.Key);
                    arrayIndex++;
                }
            }

            using (var enumerator = _underlyingBase.JsonValues.GetEnumerator()) {
                while (arrayIndex < arrayLength && enumerator.MoveNext()) {
                    array[arrayIndex] = enumerator.Current.Key;
                    arrayIndex++;
                }
            }
        }
    }
} // end of namespace